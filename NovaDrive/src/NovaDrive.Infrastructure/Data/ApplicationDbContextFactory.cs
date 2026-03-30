namespace NovaDrive.Infrastructure.Data;

internal sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Fall back to a local dev connection string when no environment variable is set.
        // In CI / Docker the real connection string is injected via environment variables.
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=novadrive;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name)
        );

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
