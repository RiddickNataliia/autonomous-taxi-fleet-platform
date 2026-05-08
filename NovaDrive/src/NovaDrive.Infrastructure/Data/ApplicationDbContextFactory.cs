namespace NovaDrive.Infrastructure.Data;

internal sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=novadrive;Username=postgres;Password=postgres"; // Fall back 

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name)
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
