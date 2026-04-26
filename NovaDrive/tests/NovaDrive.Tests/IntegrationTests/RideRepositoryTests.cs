namespace NovaDrive.Tests.IntegrationTests;

public class RideRepositoryTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = default!;
    private ApplicationDbContext _ctx = default!;
    private RideRepository _rideRepo = default!;
    private VehicleRepository _vehicleRepo = default!;
    private PassengerRepository _passengerRepo = default!;
    private UserRepository _userRepo = default!;

    public async Task InitializeAsync()
    {
        (_ctx, _container) = await TestDbContextFactory.CreateAsync();
        _rideRepo      = new RideRepository(_ctx);
        _vehicleRepo   = new VehicleRepository(_ctx);
        _passengerRepo = new PassengerRepository(_ctx);
        _userRepo      = new UserRepository(_ctx);
    }

    private async Task<(Passenger passenger, Vehicle vehicle)> SeedAsync(string email, string plate, string vin)
    {
        var user      = User.Create(email, $"auth0|{Guid.NewGuid()}", UserRole.Passenger);
        var passenger = Passenger.Create(user.Id);
        passenger.UpdateProfile("Rider", "1 Street", PaymentMethod.CreditCard);
        await _userRepo.Add(user);
        await _passengerRepo.Add(passenger);

        var vehicle = Vehicle.Create(plate, "Ride Car", new Vin(vin), 2023, VehicleType.Standard);
        await _vehicleRepo.Add(vehicle);

        await _ctx.SaveChangesAsync();
        return (passenger, vehicle);
    }

    [Fact]
    public async Task Add_And_GetByPassengerId_ReturnsRide()
    {
        var (passenger, vehicle) = await SeedAsync("rider1@test.com", "1-RDE-001", "JH4KA7650MC020001");

        var ride = new Ride
        {
            PassengerId = passenger.Id,
            VehicleId   = vehicle.Id,
            Departure   = "Brussels Central",
            Destination = "Brussels Airport"
        };
        ride.RequestRide();

        await _rideRepo.Add(ride);
        await _ctx.SaveChangesAsync();

        var rides = await _rideRepo.GetByPassengerId(passenger.Id);

        rides.Should().Contain(r => r.Id == ride.Id);
        rides.First(r => r.Id == ride.Id).Status.Should().Be(RideStatus.Requested);
    }

    [Fact]
    public async Task GetActiveByPassengerId_ReturnsOnlyActiveRide()
    {
        var (passenger, vehicle) = await SeedAsync("rider2@test.com", "1-RDE-002", "JH4KA7650MC020002");

        var ride = new Ride
        {
            PassengerId = passenger.Id,
            VehicleId   = vehicle.Id,
            Departure   = "A",
            Destination = "B"
        };
        ride.RequestRide();

        await _rideRepo.Add(ride);
        await _ctx.SaveChangesAsync();

        var active = await _rideRepo.GetActiveByPassengerId(passenger.Id);

        active.Should().NotBeNull();
        active!.Status.Should().Be(RideStatus.Requested);
    }

    [Fact]
    public async Task GetByVehicleId_ReturnsRidesForVehicle()
    {
        var (passenger, vehicle) = await SeedAsync("rider3@test.com", "1-RDE-003", "JH4KA7650MC020003");

        var ride = new Ride
        {
            PassengerId = passenger.Id,
            VehicleId   = vehicle.Id,
            Departure   = "X",
            Destination = "Y"
        };
        ride.RequestRide();

        await _rideRepo.Add(ride);
        await _ctx.SaveChangesAsync();

        var results = await _rideRepo.GetByVehicleId(vehicle.Id);

        results.Should().Contain(r => r.VehicleId == vehicle.Id);
    }

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
        await _container.DisposeAsync();
    }
}