namespace NovaDrive.Tests.IntegrationTests;

public class SensorDiagnosticRepositoryTests : IAsyncLifetime
{
    private readonly MongoDbContainer _container = new MongoDbBuilder("mongo:8.0").Build();
    private SensorDiagnosticRepository? _repo;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        try { BsonSerializer.RegisterSerializer(new GuidSerializer(MongoDB.Bson.GuidRepresentation.Standard)); }
        catch (BsonSerializationException) { }

        var db = new MongoClient(_container.GetConnectionString()).GetDatabase("novadrive_test");
        _repo = new SensorDiagnosticRepository(db);
    }

    [Fact]
    public async Task Add_ShouldPersistDiagnostic()
    {
        var vehicleId  = Guid.NewGuid();
        var diagnostic = SensorDiagnostic.Create(vehicleId, SensorType.Lidar, "LIDAR_TEMP_HIGH", DiagnosticSeverity.Warning, null);

        await _repo!.Add(diagnostic);
        var results = await _repo.GetByVehicleId(vehicleId);

        results.Should().ContainSingle();
        results.First().ErrorCode.Should().Be("LIDAR_TEMP_HIGH");
    }

    [Fact]
    public async Task GetBySeverity_ShouldReturnMatchingDiagnostics()
    {
        var vehicleId = Guid.NewGuid();
        await _repo!.Add(SensorDiagnostic.Create(vehicleId, SensorType.Radar,  "RADAR_SIGNAL_WEAK",    DiagnosticSeverity.Warning,  null));
        await _repo!.Add(SensorDiagnostic.Create(vehicleId, SensorType.Camera, "CAM_BLUR_DETECTED",    DiagnosticSeverity.Error,    null));
        await _repo!.Add(SensorDiagnostic.Create(vehicleId, SensorType.Lidar,  "LIDAR_RETURN_LOSS",    DiagnosticSeverity.Critical, null));

        var warnings = await _repo.GetBySeverity(DiagnosticSeverity.Warning);

        warnings.Should().Contain(d => d.ErrorCode == "RADAR_SIGNAL_WEAK");
        warnings.Should().NotContain(d => d.ErrorCode == "CAM_BLUR_DETECTED");
    }

    [Fact]
    public async Task GetByVehicleId_ShouldReturnOnlyThatVehicle()
    {
        var vehicle1 = Guid.NewGuid();
        var vehicle2 = Guid.NewGuid();

        await _repo!.Add(SensorDiagnostic.Create(vehicle1, SensorType.Lidar, "CODE_1", DiagnosticSeverity.Info, null));
        await _repo!.Add(SensorDiagnostic.Create(vehicle2, SensorType.Radar, "CODE_2", DiagnosticSeverity.Info, null));

        var results = await _repo.GetByVehicleId(vehicle1);

        results.Should().AllSatisfy(d => d.VehicleId.Should().Be(vehicle1));
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}