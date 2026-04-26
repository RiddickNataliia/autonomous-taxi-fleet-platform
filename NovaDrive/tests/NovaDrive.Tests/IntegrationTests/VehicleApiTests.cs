
namespace NovaDrive.Tests.IntegrationTests;

public class VehicleApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("novadrive_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:8.0")
        .Build();

    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _mongo.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace Postgres with test container
                    services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                    services.RemoveAll<ApplicationDbContext>();
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseNpgsql(_postgres.GetConnectionString()));

                    // Replace MongoDB with test container
                    services.RemoveAll<IMongoDatabase>();
                    services.AddSingleton<IMongoDatabase>(_ =>
                    {
                        var client = new MongoClient(_mongo.GetConnectionString());
                        return client.GetDatabase("novadrive_test");
                    });
                });

                // Override config so Auth0 + email don't fail
                builder.UseSetting("Auth0:Domain",   "test.auth0.com");
                builder.UseSetting("Auth0:Audience", "https://novadrive-api-test");
                builder.UseSetting("Email:SmtpHost", "localhost");
                builder.UseSetting("Email:SmtpPort", "1025");
            });

        // Apply migrations
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetVehicles_WithoutToken_Returns401()
    {
        // Act
        var response = await _client!.GetAsync("/api/v1/vehicles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHealth_Returns200()
    {
        // Act
        var response = await _client!.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostVehicle_WithoutToken_Returns401()
    {
        // Arrange
        var payload = new
        {
            vin               = "JH4KA7650MC000099",
            licensePlate      = "9-TST-999",
            modelName         = "Test Car",
            yearOfManufacture = 2023,
            vehicleType       = "Standard"
        };

        // Act
        var response = await _client!.PostAsJsonAsync("/api/v1/vehicles", payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GraphQL_WithoutToken_Returns401()
    {
        // Act
        var response = await _client!.PostAsJsonAsync("/graphql",
            new { query = "{ vehicles { id } }" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null) await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
        await _mongo.DisposeAsync();
    }
}