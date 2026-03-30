namespace NovaDrive.Infrastructure;

public static class InfrastructureService
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name)
            )
        );

        // Repositories (Scoped — one instance per HTTP request)
        services.AddScoped<IUserRepository,         UserRepository>();
        services.AddScoped<IPassengerRepository,    PassengerRepository>();
        services.AddScoped<IVehicleRepository,      VehicleRepository>();
        services.AddScoped<IRideRepository,         RideRepository>();
        services.AddScoped<ITransactionRepository,  TransactionRepository>();
        services.AddScoped<IMaintenanceLogRepository, MaintenanceLogRepository>();
        services.AddScoped<ISupportTicketRepository,  SupportTicketRepository>();
        services.AddScoped<IDiscountCodeRepository,    DiscountCodeRepository>();
        services.AddScoped<IVehicleApiKeyRepository,   VehicleApiKeyRepository>();

        // Unit of Work
        // Scoped so it shares the same DbContext instance as the repositories above.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Payment gateway
        // Transient: stateless, safe to create per-call.
        services.AddTransient<IPaymentGateway, DemoPaymentGateway>();

        return services;
    }
}
