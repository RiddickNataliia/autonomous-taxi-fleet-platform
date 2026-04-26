namespace NovaDrive.Tests.IntegrationTests;

public class TelemetryRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder("mongo:8.0").Build();
    private TelemetryRepository? _repo;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        try
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            BsonClassMap.RegisterClassMap<GpsLocation>(cm =>
            {
                cm.MapCreator(g => new GpsLocation(g.Latitude, g.Longitude));
                cm.MapProperty(g => g.Latitude);
                cm.MapProperty(g => g.Longitude);
            });
            BsonClassMap.RegisterClassMap<Telemetry>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(t => t.BatteryPercentage).SetSerializer(new Int32Serializer(BsonType.Int32));
                cm.GetMemberMap(t => t.Speed).SetSerializer(new DoubleSerializer(BsonType.Double));
                cm.GetMemberMap(t => t.InternalTemperature).SetSerializer(new DoubleSerializer(BsonType.Double));
            });
        }
        catch (BsonSerializationException) { /* already registered */ }

        var db = new MongoClient(_container.GetConnectionString()).GetDatabase("novadrive_test");
        _repo = new TelemetryRepository(db);
    }

    [Fact]
    public async Task Add_ShouldPersistTelemetry()
    {
        var vehicleId = Guid.NewGuid();
        var telemetry = Telemetry.Create(vehicleId, new GpsLocation(50.85, 4.35), 60.0, 85, 32.5);

        await _repo!.Add(telemetry);
        var results = await _repo.GetByVehicleId(vehicleId, limit: 10);

        results.Should().ContainSingle();
        results.First().Speed.Should().Be(60.0);
        results.First().BatteryPercentage.Should().Be(85);
    }

    [Fact]
    public async Task GetByVehicleId_ShouldRespectLimit()
    {
        var vehicleId = Guid.NewGuid();
        for (var i = 0; i < 10; i++)
            await _repo!.Add(Telemetry.Create(vehicleId, new GpsLocation(50.85, 4.35), i * 5.0, 80, 30.0));

        var results = await _repo!.GetByVehicleId(vehicleId, limit: 3);

        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByVehicleId_UnknownVehicle_ReturnsEmpty()
    {
        var results = await _repo!.GetByVehicleId(Guid.NewGuid(), limit: 10);
        results.Should().BeEmpty();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}