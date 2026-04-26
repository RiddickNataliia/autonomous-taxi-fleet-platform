namespace NovaDrive.Tests.IntegrationTests;

public class VehicleRepositoryTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = default!;
    private ApplicationDbContext _ctx = default!;
    private VehicleRepository _repo = default!;

    public async Task InitializeAsync()
    {
        (_ctx, _container) = await TestDbContextFactory.CreateAsync();
        _repo = new VehicleRepository(_ctx);
    }

    [Fact]
    public async Task Add_And_GetById_ReturnsVehicle()
    {
        var vehicle = Vehicle.Create("1-INT-001", "Test Model", new Vin("JH4KA7650MC000001"), 2023, VehicleType.Standard);

        await _repo.Add(vehicle);
        await _ctx.SaveChangesAsync();

        var found = await _repo.GetById(vehicle.Id);

        found.Should().NotBeNull();
        found!.LicensePlate.Should().Be("1-INT-001");
        found.ModelName.Should().Be("Test Model");
    }

    [Fact]
    public async Task GetByVin_ReturnsCorrectVehicle()
    {
        var vehicle = Vehicle.Create("1-INT-002", "VIN Test", new Vin("JH4KA7650MC000002"), 2023, VehicleType.Luxury);
        await _repo.Add(vehicle);
        await _ctx.SaveChangesAsync();

        var found = await _repo.GetByVin("JH4KA7650MC000002");

        found.Should().NotBeNull();
        found!.Type.Should().Be(VehicleType.Luxury);
    }

    [Fact]
    public async Task GetAll_ReturnsAllVehicles()
    {
        await _repo.Add(Vehicle.Create("1-INT-003", "Car A", new Vin("JH4KA7650MC000003"), 2023, VehicleType.Standard));
        await _repo.Add(Vehicle.Create("1-INT-004", "Car B", new Vin("JH4KA7650MC000004"), 2023, VehicleType.Van));
        await _ctx.SaveChangesAsync();

        var all = await _repo.GetAll();

        all.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Delete_RemovesVehicle()
    {
        var vehicle = Vehicle.Create("1-INT-005", "To Delete", new Vin("JH4KA7650MC000005"), 2023, VehicleType.Standard);
        await _repo.Add(vehicle);
        await _ctx.SaveChangesAsync();

        await _repo.Delete(vehicle.Id);
        await _ctx.SaveChangesAsync();

        var found = await _repo.GetById(vehicle.Id);
        found.Should().BeNull();
    }

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
        await _container.DisposeAsync();
    }
}