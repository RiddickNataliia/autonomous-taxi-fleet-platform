namespace NovaDrive.Application;

/// <summary>
/// Registers all Application-layer services into the DI container.
/// </summary>
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services
        services.AddScoped<IRideService,     RideService>();
        services.AddScoped<IPaymentService,  PaymentService>();
        services.AddScoped<IVehicleService,  VehicleService>();
        services.AddScoped<ISupportService,  SupportService>();
        services.AddScoped<IDiscountService, DiscountService>();
        services.AddScoped<ISensorDiagnosticService, SensorDiagnosticService>();
        services.AddScoped<IPassengerService, PassengerService>();
        services.AddScoped<IUserProvisioningService, UserProvisioningService>();

        // Domain services — stateless, transient
        services.AddTransient<IPricingEngine, PricingEngine>();
        services.AddTransient<IRideMatchingService, RideMatchingService>();

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly);

        return services;
    }
}
