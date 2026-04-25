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
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        BsonClassMap.RegisterClassMap<GpsLocation>(cm =>
        {
            cm.MapCreator(g => new GpsLocation(g.Latitude, g.Longitude));
            cm.MapProperty(g => g.Latitude);
            cm.MapProperty(g => g.Longitude);
        });

        BsonClassMap.RegisterClassMap<Telemetry>(cm =>
        {
            cm.AutoMap();
            cm.GetMemberMap(t => t.BatteryPercentage)
                .SetSerializer(new Int32Serializer(BsonType.Int32));
            cm.GetMemberMap(t => t.Speed)
                .SetSerializer(new DoubleSerializer(BsonType.Double));
            cm.GetMemberMap(t => t.InternalTemperature)
                .SetSerializer(new DoubleSerializer(BsonType.Double));
        });


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
