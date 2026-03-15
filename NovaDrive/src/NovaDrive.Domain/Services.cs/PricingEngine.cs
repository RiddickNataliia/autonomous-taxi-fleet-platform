// Services.cs/PricingEngine.cs
namespace NovaDrive.Domain.Services;

public class PricingEngine
{
    private const decimal StartingRate = 2.50m;
    private const decimal RatePerKm = 1.10m;
    private const decimal RatePerMinute = 0.30m;
    private const decimal VatRate = 0.21m;
    private const decimal MinimumFare = 5.00m;
    private const decimal NightSurcharge = 1.15m;
    private const decimal LoyaltyCapPercent = 0.20m;
    private const decimal PointsPerEuro = 100m;

    public PricingResult CalculateFinalPrice(
        double distanceKm,
        int durationMinutes,
        VehicleType vehicleType,
        DateTime requestTime,
        int availablePoints,
        DiscountCode? code)
    {
        // STEP 1: Base fare
        decimal baseFare = StartingRate
            + ((decimal)distanceKm * RatePerKm)
            + (durationMinutes * RatePerMinute);

        // STEP 2: Vehicle type multiplier
        decimal typeMultiplier = vehicleType switch
        {
            VehicleType.Standard => 1.0m,
            VehicleType.Van      => 1.5m,
            VehicleType.Luxury   => 2.2m,
            _ => 1.0m
        };
        decimal currentFare = baseFare * typeMultiplier;

        // STEP 3: Night surcharge (22:00 – 06:00), applied before discounts
        bool isNightRide = requestTime.Hour >= 22 || requestTime.Hour < 6;
        if (isNightRide)
            currentFare *= NightSurcharge;

        // STEP 4: Loyalty discount — €1 per 100 points, capped at 20% of fare
        decimal potentialLoyaltyDiscount = Math.Floor(availablePoints / PointsPerEuro) * 1.00m;
        decimal loyaltyCap = currentFare * LoyaltyCapPercent;
        decimal actualLoyaltyDiscount = Math.Min(potentialLoyaltyDiscount, loyaltyCap);
        int pointsSpent = (int)(Math.Floor(actualLoyaltyDiscount) * PointsPerEuro);

        currentFare -= actualLoyaltyDiscount;

        // STEP 5: Discount code — applied to remaining amount after loyalty
        decimal codeDiscount = 0m;
        if (code != null && code.IsValid(currentFare))
        {
            codeDiscount = code.CalculateDiscount(currentFare);
            currentFare -= codeDiscount;
        }

        // STEP 6: Hard business rules — no negatives, minimum fare
        currentFare = Math.Max(0m, currentFare);
        decimal netAmount = Math.Max(MinimumFare, Math.Round(currentFare, 2));

        // STEP 7: VAT on top of the net amount
        decimal vatAmount = Math.Round(netAmount * VatRate, 2);
        decimal totalGross = netAmount + vatAmount;

        return new PricingResult(
            NetAmount: netAmount,
            VatAmount: vatAmount,
            TotalGross: totalGross,
            LoyaltyDiscountApplied: actualLoyaltyDiscount,
            CodeDiscountApplied: codeDiscount,
            PointsUsed: pointsSpent
        );
    }
}