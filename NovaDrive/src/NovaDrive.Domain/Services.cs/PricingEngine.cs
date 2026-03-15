namespace NovaDrive.Domain.Services;

public class PricingEngine
{
    private const decimal StartingRate = 2.50m;
    private const decimal RatePerKm = 1.10m;
    private const decimal RatePerMinute = 0.30m;
    private const decimal NightSurchargeRate = 0.15m;

    public Money CalculateBaseFare(double distanceKm, double durationMinutes, VehicleType vehicleType, DateTimeOffset rideTime)
    {
        // 1. Basic Calculation
        decimal basePrice = StartingRate + 
                           ((decimal)distanceKm * RatePerKm) + 
                           ((decimal)durationMinutes * RatePerMinute);

        // 2. Night Rate (Applied specifically to the base price before multipliers)
        if (IsNightShift(rideTime))
        {
            basePrice += (basePrice * NightSurchargeRate);
        }

        // 3. Vehicle Multiplier
        decimal fareAfterVehicleType = basePrice * GetVehicleMultiplier(vehicleType);

        return new Money(fareAfterVehicleType, "EUR");
    }

    private bool IsNightShift(DateTimeOffset time) => time.Hour >= 22 || time.Hour < 6;

    private decimal GetVehicleMultiplier(VehicleType type) => type switch
    {
        VehicleType.Van => 1.5m,
        VehicleType.Luxury => 2.2m,
        _ => 1.0m // Standard Sedan
    };
}