//  Bootstrap logger 
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting NovaDrive API");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, services, config) => config
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/novadrive-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14));

    //  Infrastructure (Postgres + MongoDB + all repositories + UoW) 
    builder.Services.AddInfrastructure(builder.Configuration);

    //  Application services + FluentValidation 
    builder.Services.AddApplication();

    // AUTH0 AUTHENTICATION
    var auth0Domain = builder.Configuration["Auth0:Domain"]
        ?? throw new InvalidOperationException("Missing configuration: Auth0:Domain");
    var auth0Audience = builder.Configuration["Auth0:Audience"]
        ?? throw new InvalidOperationException("Missing configuration: Auth0:Audience");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://{auth0Domain}/";
            options.Audience  = auth0Audience;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidIssuer              = $"https://{auth0Domain}/",
                ValidateAudience         = true,
                ValidAudience            = auth0Audience,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true
            };
        });

    // AUTHORIZATION POLICIES
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("read:rides",      p => p.RequireAssertion(ctx => HasScopeOrPermission(ctx.User, "read:rides")));
        options.AddPolicy("create:rides",    p => p.RequireAssertion(ctx => HasScopeOrPermission(ctx.User, "create:rides")));
        options.AddPolicy("read:profile",    p => p.RequireAssertion(ctx => HasScopeOrPermission(ctx.User, "read:profile")));
        options.AddPolicy("update:profile",  p => p.RequireAssertion(ctx => HasScopeOrPermission(ctx.User, "update:profile")));
        options.AddPolicy("admin:fleet",     p => p.RequireAssertion(ctx => HasScopeOrPermission(ctx.User, "admin:fleet")));
        options.AddPolicy("admin:users",     p => p.RequireAssertion(ctx => HasScopeOrPermission(ctx.User, "admin:users")));
        options.AddPolicy("admin:support",   p => p.RequireAssertion(ctx => HasScopeOrPermission(ctx.User, "admin:support")));
        options.AddPolicy("admin:discounts", p => p.RequireAssertion(ctx => HasScopeOrPermission(ctx.User, "admin:discounts")));
    });

    //  OpenAPI / Swagger \
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();

    // Health checks 
    builder.Services.AddHealthChecks();

    builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

    //  CORS 
    builder.Services.AddCors(options =>
        options.AddDefaultPolicy(policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader()));
                  
    //  JSON options (e.g. for serializing enums as strings in API responses)
    builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    //  Build 
    var app = builder.Build();

    //  Auto-migrate on startup 
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider
            .GetRequiredService<NovaDrive.Infrastructure.Data.ApplicationDbContext>();

        Log.Information("Applying EF Core migrations...");
        await db.Database.MigrateAsync();
        Log.Information("Migrations applied.");
    }

    // MIDDLEWARE PIPELINE
    app.UseExceptionHandling(); // Custom middleware to catch unhandled exceptions

    app.UseSerilogRequestLogging(opts =>
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000} ms");

    if (app.Environment.IsDevelopment())
        app.MapOpenApi();

    app.UseHttpsRedirection();
    app.UseCors();
    app.UseVehicleApiKeyAuth(); // Custom middleware for authenticating vehicles using API keys, done before standard auth to allow vehicle simulators to authenticate without user tokens
    app.UseAuthentication();
    app.UseAuthorization();

    // ENDPOINTS
    app.MapHealthChecks("/health");

    // Callback endpoint for Auth0 Authorization Code flow testing
    app.MapGet("/callback", (string? code, string? state, string? error, string? error_description) =>
    {
        if (!string.IsNullOrEmpty(error))
            return Results.BadRequest($"Auth0 Error: {error}\nDescription: {error_description}");

        if (string.IsNullOrEmpty(code))
            return Results.BadRequest("No code returned from Auth0. Please ensure you are logging in via the authorization URL.");

        var html = $"""
            <html>
            <head><title>Auth0 Callback</title></head>
            <body style="font-family: sans-serif; max-width: 600px; margin: 40px auto; padding: 20px; border: 1px solid #ccc; border-radius: 8px;">
                <h1 style="color: #4CAF50;">Authorization Successful!</h1>
                <p>You have successfully logged in.</p>
                <hr />
                <p><strong>Your Authorization Code:</strong></p>
                <div style="background: #f4f4f4; padding: 15px; border-radius: 4px; border: 1px solid #ddd; overflow-wrap: break-word; font-family: monospace;">
                    {code}
                </div>
                <p>Copy this code and paste it into your <code>.http</code> file to exchange it for an access token.</p>
            </body>
            </html>
            """;

        return Results.Content(html, "text/html");
    })
    .WithName("CallbackEndpoint")
    .ExcludeFromDescription();

    app.MapGet("/", () => Results.Redirect("/health")).ExcludeFromDescription();

    // Endpoint groups 
    var v1 = app.MapGroup("/api/v1");

    //  v1.MapUserEndpoints
    v1.MapGroup("/passengers")
    .MapPassengerEndpoints();

    //   v1.MapVehicleEndpoints
    v1.MapGroup("/vehicles")
    .MapVehicleEndpoints();

    //   v1.MapRideEndpoints
    v1.MapGroup("/rides")
    .MapRideEndpoints();
    //   v1.MapPaymentEndpoints
    v1.MapGroup("/payments")
    .MapPaymentEndpoints();

    v1.MapGroup("/telemetry")
    .MapTelemetryEndpoints();

    v1.MapGroup("/diagnostics").
    MapSensorDiagnosticEndpoints();
    
    v1.MapGroup("/support")
    .MapSupportTicketEndpoints();
    
    v1.MapGroup("/discounts")
    .MapDiscountCodeEndpoints();

    
    
    app.Run();
    }
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "NovaDrive API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// LOCAL FUNCTIONS

static bool HasScopeOrPermission(ClaimsPrincipal user, string required)
{
    var inScope = user.Claims
        .Where(c => c.Type == "scope")
        .SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        .Any(value => string.Equals(value, required, StringComparison.OrdinalIgnoreCase));

    if (inScope) return true;

    foreach (var claim in user.Claims.Where(c => c.Type == "permissions"))
    {
        if (string.Equals(claim.Value, required, StringComparison.OrdinalIgnoreCase))
            return true;

        foreach (var value in claim.Value.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
            if (string.Equals(value, required, StringComparison.OrdinalIgnoreCase))
                return true;

        if (claim.Value.StartsWith('['))
        {
            try
            {
                using var doc = JsonDocument.Parse(claim.Value);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    foreach (var item in doc.RootElement.EnumerateArray())
                        if (item.ValueKind == JsonValueKind.String &&
                            string.Equals(item.GetString(), required, StringComparison.OrdinalIgnoreCase))
                            return true;
            }
            catch { }
        }
    }
    return false;
}