namespace NovaDrive.Tests.IntegrationTests;

public class PassengerRepositoryTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = default!;
    private ApplicationDbContext _ctx = default!;
    private PassengerRepository _repo = default!;
    private UserRepository _userRepo = default!;

    public async Task InitializeAsync()
    {
        (_ctx, _container) = await TestDbContextFactory.CreateAsync();
        _repo     = new PassengerRepository(_ctx);
        _userRepo = new UserRepository(_ctx);
    }

    private async Task<(User user, Passenger passenger)> SeedAsync(string email)
    {
        var user      = User.Create(email, $"auth0|{Guid.NewGuid()}", UserRole.Passenger);
        var passenger = Passenger.Create(user.Id);
        passenger.UpdateProfile("Test User", "123 Main St", PaymentMethod.CreditCard);

        await _userRepo.Add(user);
        await _repo.Add(passenger);
        await _ctx.SaveChangesAsync();

        return (user, passenger);
    }

    [Fact]
    public async Task Add_And_GetById_ReturnsPassenger()
    {
        var (_, passenger) = await SeedAsync("passenger1@test.com");

        var found = await _repo.GetById(passenger.Id);

        found.Should().NotBeNull();
        found!.FullName.Should().Be("Test User");
    }

    [Fact]
    public async Task GetByUserId_ReturnsPassenger()
    {
        var (user, passenger) = await SeedAsync("passenger2@test.com");

        var found = await _repo.GetByUserId(user.Id);

        found.Should().NotBeNull();
        found!.Id.Should().Be(passenger.Id);
    }

    [Fact]
    public async Task GetAll_ReturnsAllPassengers()
    {
        await SeedAsync("passenger3@test.com");
        await SeedAsync("passenger4@test.com");

        var all = await _repo.GetAll();

        all.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
        await _container.DisposeAsync();
    }
}