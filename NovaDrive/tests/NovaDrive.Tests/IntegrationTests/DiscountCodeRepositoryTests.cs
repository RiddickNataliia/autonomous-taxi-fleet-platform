namespace NovaDrive.Tests.IntegrationTests;

public class DiscountCodeRepositoryTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = default!;
    private ApplicationDbContext _ctx = default!;
    private DiscountCodeRepository _repo = default!;

    public async Task InitializeAsync()
    {
        (_ctx, _container) = await TestDbContextFactory.CreateAsync();
        _repo = new DiscountCodeRepository(_ctx);
    }

    [Fact]
    public async Task Add_And_GetByCode_ReturnsCode()
    {
    var code = DiscountCode.Create("SUMMER24", DiscountType.Percentage, 15m, 10m,
        DateTimeOffset.UtcNow.AddDays(30));

        await _repo.Add(code);
        await _ctx.SaveChangesAsync();

        var found = await _repo.GetByCode("SUMMER24");

        found.Should().NotBeNull();
        found!.Value.Should().Be(15m);
        found.Type.Should().Be(DiscountType.Percentage);
    }

    [Fact]
    public async Task GetByCode_IsCaseInsensitive()
    {
    var code = DiscountCode.Create("WELCOME5", DiscountType.Flat, 5m, 10m,
        DateTimeOffset.UtcNow.AddDays(30));

        await _repo.Add(code);
        await _ctx.SaveChangesAsync();

        var found = await _repo.GetByCode("welcome5");

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByCode_NonExistent_ReturnsNull()
    {
        var found = await _repo.GetByCode("DOESNOTEXIST");
        found.Should().BeNull();
    }

    public async Task DisposeAsync()
    {
        await _ctx.DisposeAsync();
        await _container.DisposeAsync();
    }
}