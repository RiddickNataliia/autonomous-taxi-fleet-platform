namespace NovaDrive.Tests.IntegrationTests;

public static class TestDbContextFactory
{
    public static async Task<(ApplicationDbContext ctx, PostgreSqlContainer container)> CreateAsync()
    {
        var container = new PostgreSqlBuilder("postgres:18-alpine")
            .WithDatabase("novadrive_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await container.StartAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(container.GetConnectionString())
            .Options;

        var ctx = new ApplicationDbContext(options);
        await ctx.Database.MigrateAsync();

        return (ctx, container);
    }
}