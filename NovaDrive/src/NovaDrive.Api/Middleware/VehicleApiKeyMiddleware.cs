namespace NovaDrive.Api.Middleware;

/// <summary>
/// Validates the X-Api-Key header for vehicle-facing endpoints.
/// Runs before UseAuthentication so vehicle requests bypass Auth0 entirely.
///
/// Protected paths:
///   POST /api/v1/telemetry
///   POST /api/v1/diagnostics
///
/// On success: attaches the authenticated Vehicle to HttpContext.Items["AuthenticatedVehicle"]
/// so endpoints can use it without a second database round-trip.
///
/// On failure: short-circuits with 401 before any endpoint logic runs.
/// </summary>
public class VehicleApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<VehicleApiKeyMiddleware> _logger;

    // Paths that require vehicle API key authentication
    private static readonly string[] ProtectedPaths =
    [
        "/api/v1/telemetry",
        "/api/v1/diagnostics",
        "/api/v1/vehicles/vitals"
    ];

    // Specific ride actions done by vehicle
    private static readonly string[] ProtectedSuffixes =
    [
        "/start",
        "/complete"
    ];

    public VehicleApiKeyMiddleware(
        RequestDelegate next,
        ILogger<VehicleApiKeyMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only intercept vehicle-facing endpoints
        var path = context.Request.Path.Value ?? string.Empty;
        var isProtected = ProtectedPaths.Any(p =>
            path.StartsWith(p, StringComparison.OrdinalIgnoreCase))
            || ProtectedSuffixes.Any(s =>
            path.EndsWith(s, StringComparison.OrdinalIgnoreCase));

        if (!isProtected)
        {
            await _next(context);
            return;
        }

        // Both headers are required
        if (!context.Request.Headers.TryGetValue("X-Api-Key", out var providedKey)
            || string.IsNullOrWhiteSpace(providedKey))
        {
            await WriteUnauthorized(context, "Missing X-Api-Key header.");
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Vehicle-Id", out var vehicleIdHeader)
            || string.IsNullOrWhiteSpace(vehicleIdHeader))
        {
            await WriteUnauthorized(context, "Missing X-Vehicle-Id header.");
            return;
        }

        if (!Guid.TryParse(vehicleIdHeader, out var vehicleId))
        {
            await WriteBadRequest(context, "X-Vehicle-Id must be a valid GUID.");
            return;
        }

        // Resolve IVehicleRepository from the request scope
        // (cannot inject in constructor — middleware is singleton, repo is scoped)
        var vehicleRepo = context.RequestServices
            .GetRequiredService<IVehicleRepository>();

        var vehicle = await vehicleRepo.GetById(vehicleId);

        if (vehicle is null)
        {
            _logger.LogWarning("Vehicle API key auth failed — vehicle {VehicleId} not found", vehicleId);
            await WriteUnauthorized(context, "Unknown vehicle.");
            return;
        }

        if (string.IsNullOrEmpty(vehicle.ApiKeyHash))
        {
            _logger.LogWarning("Vehicle API key auth failed — vehicle {VehicleId} has no key provisioned", vehicleId);
            await WriteUnauthorized(context, "No API key provisioned for this vehicle.");
            return;
        }

        // BCrypt.Verify compares the plain key against the stored hash
        // This is the only place BCrypt is called in the middleware layer
        var keyIsValid = BCrypt.Net.BCrypt.Verify(providedKey!, vehicle.ApiKeyHash);

        if (!keyIsValid)
        {
            _logger.LogWarning("Vehicle API key auth failed — invalid key for vehicle {VehicleId}", vehicleId);
            await WriteUnauthorized(context, "Invalid API key.");
            return;
        }

        // Attach authenticated vehicle to context — endpoints read it from here
        context.Items["AuthenticatedVehicle"] = vehicle;
        _logger.LogInformation("Vehicle {VehicleId} authenticated via API key", vehicleId);

        await _next(context);
    }

    private static Task WriteUnauthorized(HttpContext context, string message)
    {
        context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(new
        {
            status  = 401,
            title   = "Unauthorized",
            detail  = message,
            instance = context.Request.Path.Value
        });
    }

    private static Task WriteBadRequest(HttpContext context, string message)
    {
        context.Response.StatusCode  = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(new
        {
            status  = 400,
            title   = "Bad Request",
            detail  = message,
            instance = context.Request.Path.Value
        });
    }
}

/// <summary>Extension method for clean registration in Program.cs.</summary>
public static class VehicleApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseVehicleApiKeyAuth(this IApplicationBuilder app)
        => app.UseMiddleware<VehicleApiKeyMiddleware>();
}