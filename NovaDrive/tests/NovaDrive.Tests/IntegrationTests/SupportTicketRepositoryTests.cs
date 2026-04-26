namespace NovaDrive.Tests.IntegrationTests;

public class SupportTicketRepositoryTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = default!;
    private ApplicationDbContext _ctx = default!;
    private SupportTicketRepository _repo = default!;
    private PassengerRepository _passengerRepo = default!;
    private UserRepository _userRepo = default!;

    public async Task InitializeAsync()
    {
        (_ctx, _container) = await TestDbContextFactory.CreateAsync();
        _repo          = new SupportTicketRepository(_ctx);
        _passengerRepo = new PassengerRepository(_ctx);
        _userRepo      = new UserRepository(_ctx);
    }

    private async Task<Passenger> SeedPassengerAsync(string email)
    {
        var user      = User.Create(email, $"auth0|{Guid.NewGuid()}", UserRole.Passenger);
        var passenger = Passenger.Create(user.Id);
        await _userRepo.Add(user);
        await _passengerRepo.Add(passenger);
        await _ctx.SaveChangesAsync();
        return passenger;
    }

    [Fact]
    public async Task Add_And_GetById_ReturnsTicket()
    {
        var passenger = await SeedPassengerAsync("support1@test.com");
        var ticket    = SupportTicket.Create(passenger.Id, "App crashed", "The app crashed on login.");

        await _repo.Add(ticket);
        await _ctx.SaveChangesAsync();

        var found = await _repo.GetById(ticket.Id);

        found.Should().NotBeNull();
        found!.Subject.Should().Be("App crashed");
        found.Status.Should().Be(TicketStatus.Open);
    }

    [Fact]
    public async Task GetByPassengerId_ReturnsOnlyThatPassengersTickets()
    {
        var p1 = await SeedPassengerAsync("support2@test.com");
        var p2 = await SeedPassengerAsync("support3@test.com");

        await _repo.Add(SupportTicket.Create(p1.Id, "Issue A", "Description A"));
        await _repo.Add(SupportTicket.Create(p2.Id, "Issue B", "Description B"));
        await _ctx.SaveChangesAsync();

        var tickets = await _repo.GetByPassengerId(p1.Id);

        tickets.Should().AllSatisfy(t => t.PassengerId.Should().Be(p1.Id));
    }

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
        await _container.DisposeAsync();
    }
}