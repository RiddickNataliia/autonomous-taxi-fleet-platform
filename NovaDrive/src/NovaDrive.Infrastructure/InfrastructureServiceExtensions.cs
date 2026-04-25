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

        // MongoDB
        services.AddSingleton<IMongoDatabase>(sp =>
        {
            var client = new MongoClient(configuration.GetConnectionString("Mongo"));
            return client.GetDatabase("novadrive");
        });

        services.AddScoped<IInvoiceService, PdfInvoiceService>();
        services.AddScoped<IEmailService, MailKitEmailService>();

        // Repositories (Scoped — one instance per HTTP request)
        services.AddScoped<IUserRepository,         UserRepository>();
        services.AddScoped<IPassengerRepository,    PassengerRepository>();
        services.AddScoped<IVehicleRepository,      VehicleRepository>();
        services.AddScoped<IRideRepository,         RideRepository>();
        services.AddScoped<ITransactionRepository,  TransactionRepository>();
        services.AddScoped<IMaintenanceLogRepository, MaintenanceLogRepository>();
        services.AddScoped<ISupportTicketRepository,  SupportTicketRepository>();
        services.AddScoped<IDiscountCodeRepository,    DiscountCodeRepository>();
        services.AddScoped<ISensorDiagnosticRepository, SensorDiagnosticRepository>();
        services.AddScoped<ITelemetryRepository,        TelemetryRepository>();

        // Payment gateway (Transient — stateless, safe to create per-call)
        services.AddScoped<IPaymentGateway, DemoPaymentGateway>();
        

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
