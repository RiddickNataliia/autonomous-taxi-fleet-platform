namespace NovaDrive.Tests.IntegrationTests;

public class MaintenanceLogRepositoryTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = default!;
    private ApplicationDbContext _ctx = default!;
    private MaintenanceLogRepository _repo = default!;
    private VehicleRepository _vehicleRepo = default!;

    public async Task InitializeAsync()
    {
        (_ctx, _container) = await TestDbContextFactory.CreateAsync();
        _repo        = new MaintenanceLogRepository(_ctx);
        _vehicleRepo = new VehicleRepository(_ctx);
    }

    private async Task<Vehicle> SeedVehicleAsync(string plate, string vin)
    {
        var vehicle = Vehicle.Create(plate, "Test Car", new Vin(vin), 2023, VehicleType.Standard);
        await _vehicleRepo.Add(vehicle);
        await _ctx.SaveChangesAsync();
        return vehicle;
    }

    [Fact]
    public async Task Add_And_GetByVehicleId_ReturnsLog()
    {
        var vehicle = await SeedVehicleAsync("1-MNT-001", "JH4KA7650MC010001");
        var log     = MaintenanceLog.Create(vehicle.Id, "Oil change", "John Smith", 150m);

        await _repo.Add(log);
        await _ctx.SaveChangesAsync();

        var results = await _repo.GetByVehicleId(vehicle.Id);

        results.Should().ContainSingle();
        results.First().Description.Should().Be("Oil change");
        results.First().TechnicianName.Should().Be("John Smith");
    }

    [Fact]
    public async Task GetByVehicleId_ReturnsOnlyThatVehiclesLogs()
    {
        var v1 = await SeedVehicleAsync("1-MNT-002", "JH4KA7650MC010002");
        var v2 = await SeedVehicleAsync("1-MNT-003", "JH4KA7650MC010003");

        await _repo.Add(MaintenanceLog.Create(v1.Id, "Brake check", "Tech A", 100m));
        await _repo.Add(MaintenanceLog.Create(v2.Id, "Tire change", "Tech B", 200m));
        await _ctx.SaveChangesAsync();

        var results = await _repo.GetByVehicleId(v1.Id);

        results.Should().AllSatisfy(l => l.VehicleId.Should().Be(v1.Id));
    }

    [Fact]
    public async Task Create_WithNegativeCost_ThrowsDomainException()
    {
        var act = () => MaintenanceLog.Create(Guid.NewGuid(), "Bad log", "Tech", -50m);
        act.Should().Throw<Exception>();
    }

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
        await _container.DisposeAsync();
    }
}