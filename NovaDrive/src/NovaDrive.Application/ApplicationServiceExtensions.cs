namespace NovaDrive.Application;

/// <summary>
/// Registers all Application-layer services into the DI container.
/// Call from Program.cs:  builder.Services.AddApplication();
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

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ApplicationServiceExtensions).Assembly);

        return services;
    }
}
