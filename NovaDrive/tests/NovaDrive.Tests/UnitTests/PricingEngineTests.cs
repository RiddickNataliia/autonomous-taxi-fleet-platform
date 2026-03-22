namespace NovaDrive.Tests.UnitTests;
public class PricingEngineTests
{
    private readonly PricingEngine _engine = new();

    [Fact]
    public void StandardRide_Daytime_NoDiscounts_ReturnsCorrectBreakdown()
    {
        // Base fare: €2.50 + (10km × €1.10) + (20min × €0.30) = €19.50
        // VAT: €19.50 × 0.21 = €4.095 → €4.10
        // Gross: €19.50 + €4.10 = €23.60
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 14, 0, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(19.50m, result.NetAmount);
        Assert.Equal(4.10m,  result.VatAmount);
        Assert.Equal(23.60m, result.TotalGross);
        Assert.Equal(0m,     result.LoyaltyDiscountApplied);
        Assert.Equal(0m,     result.CodeDiscountApplied);
        Assert.Equal(0,      result.PointsUsed);
    }

    [Fact]
    public void ZeroDistanceAndDuration_OnlyStartingRateApplied()
    {
        // Base fare: €2.50 + €0 + €0 = €2.50 → below minimum → €5.00
        // VAT: €5.00 × 0.21 = €1.05
        // Gross: €6.05
        var result = _engine.CalculateFinalPrice(
            distanceKm:      0.0,
            durationMinutes: 0,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(5.00m, result.NetAmount);
        Assert.Equal(1.05m, result.VatAmount);
        Assert.Equal(6.05m, result.TotalGross);
    }

    [Fact]
    public void StandardRide_Nighttime_NoDiscounts_AppliesSurcharge()
    {
        // Base fare: €19.50
        // Night surcharge: €19.50 × 1.15 = €22.425 → €22.43
        // VAT: €22.43 × 0.21 = €4.7103 → €4.71
        // Gross: €22.43 + €4.71 = €27.14
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 22, 0, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(22.43m, result.NetAmount);
        Assert.Equal(4.71m,  result.VatAmount);
        Assert.Equal(27.14m, result.TotalGross);
    }

    [Fact]
    public void StandardRide_JustBeforeMidnight_AppliesNightSurcharge()
    {
        // 23:59 is still nighttime
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 23, 59, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(22.43m, result.NetAmount);
    }

    [Fact]
    public void StandardRide_JustBefore6AM_AppliesNightSurcharge()
    {
        // 05:59 is still nighttime
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 5, 59, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(22.43m, result.NetAmount);
    }

    [Fact]
    public void RideAtExactlyMidnight_AppliesNightSurcharge()
    {
        // 00:00 is nighttime
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 0, 0, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(22.43m, result.NetAmount);
    }

    [Fact]
    public void StandardRide_At6AM_DoesNotApplyNightSurcharge()
    {
        // 06:00 exactly is daytime
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 6, 0, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(19.50m, result.NetAmount);
    }

    [Fact]
    public void StandardRide_At2159_DoesNotApplyNightSurcharge()
    {
        // 21:59 is still daytime
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 21, 59, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(19.50m, result.NetAmount);
    }

    [Fact]
    public void VanRide_Daytime_NoDiscounts_AppliesVanMultiplier()
    {
        // Base fare: €19.50 × 1.5 = €29.25
        // VAT: €29.25 × 0.21 = €6.1425 → €6.14
        // Gross: €29.25 + €6.14 = €35.39
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Van,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(29.25m, result.NetAmount);
        Assert.Equal(6.14m,  result.VatAmount);
        Assert.Equal(35.39m, result.TotalGross);
    }

    [Fact]
    public void LuxuryRide_Daytime_NoDiscounts_AppliesLuxuryMultiplier()
    {
        // Base fare: €19.50 × 2.2 = €42.90
        // VAT: €42.90 × 0.21 = €9.009 → €9.01
        // Gross: €42.90 + €9.01 = €51.91
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Luxury,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(42.90m, result.NetAmount);
        Assert.Equal(9.01m,  result.VatAmount);
        Assert.Equal(51.91m, result.TotalGross);
    }

    [Fact]
    public void UnknownVehicleType_DefaultsToStandardMultiplier()
    {
        var resultUnknown = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Unknown,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            null
        );

        var resultStandard = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(resultStandard.NetAmount, resultUnknown.NetAmount);
    }

    [Fact]
    public void StandardRide_WithLoyaltyPoints_AppliesDiscount()
    {
        // Base fare: €19.50
        // 200 points = €2.00 discount
        // Loyalty cap: €19.50 × 0.20 = €3.90 — €2.00 under cap
        // Net after loyalty: €19.50 - €2.00 = €17.50
        // VAT: €17.50 × 0.21 = €3.675 → €3.68
        // Gross: €17.50 + €3.68 = €21.18
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 200,
            code:            null
        );

        Assert.Equal(17.50m, result.NetAmount);
        Assert.Equal(3.68m,  result.VatAmount);
        Assert.Equal(21.18m, result.TotalGross);
        Assert.Equal(2.00m,  result.LoyaltyDiscountApplied);
        Assert.Equal(200,    result.PointsUsed);
    }

    [Fact]
    public void LoyaltyPoints_Below100_NoDiscountApplied()
    {
        // 99 points is not enough for any discount
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 99,
            code:            null
        );

        Assert.Equal(19.50m, result.NetAmount);
        Assert.Equal(0m,     result.LoyaltyDiscountApplied);
        Assert.Equal(0,      result.PointsUsed);
    }

    [Fact]
    public void StandardRide_LoyaltyDiscountCappedAt20Percent()
    {
        // Base fare: €19.50
        // 10000 points = potential €100 discount
        // Loyalty cap: €19.50 × 0.20 = €3.90 — capped at €3.90
        // Points spent: (3.90) × 100 = 390
        // Net after loyalty: €19.50 - €3.90 = €15.60
        // VAT: €15.60 × 0.21 = €3.276 → €3.28
        // Gross: €15.60 + €3.28 = €18.88
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 10000,
            code:            null
        );

        Assert.Equal(15.60m, result.NetAmount);
        Assert.Equal(3.28m,  result.VatAmount);
        Assert.Equal(18.88m, result.TotalGross);
        Assert.Equal(3.90m,  result.LoyaltyDiscountApplied);
        Assert.Equal(390,    result.PointsUsed);
    }

    [Fact]
    public void StandardRide_FlatPromoCode_AppliesAfterLoyalty()
    {
        // Base fare: €19.50, no loyalty points
        // Flat code: €5.00 off → €14.50
        // VAT: €14.50 × 0.21 = €3.045 → €3.05
        // Gross: €14.50 + €3.05 = €17.55
        var code = new DiscountCode("WELCOME5", DiscountType.Flat, 5.00m, 10.00m,
            DateTimeOffset.UtcNow.AddDays(30));

        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            code
        );

        Assert.Equal(14.50m, result.NetAmount);
        Assert.Equal(3.05m,  result.VatAmount);
        Assert.Equal(17.55m, result.TotalGross);
        Assert.Equal(5.00m,  result.CodeDiscountApplied);
    }

    [Fact]
    public void StandardRide_PercentagePromoCode_AppliesAfterLoyalty()
    {
        // Base fare: €19.50, no loyalty points
        // 15% code: €19.50 × 0.15 = €2.925 → €2.93
        // Net after code: €19.50 - €2.93 = €16.57
        // VAT: €16.57 × 0.21 = €3.4797 → €3.48
        // Gross: €16.57 + €3.48 = €20.05
        var code = new DiscountCode("SUMMER24", DiscountType.Percentage, 15.00m, 10.00m,
            DateTimeOffset.UtcNow.AddDays(30));

        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            code
        );

        Assert.Equal(16.57m, result.NetAmount);
        Assert.Equal(3.48m,  result.VatAmount);
        Assert.Equal(20.05m, result.TotalGross);
        Assert.Equal(2.93m,  result.CodeDiscountApplied);
    }

    [Fact]
    public void StandardRide_LoyaltyThenPromoCode_AppliedInCorrectOrder()
    {
        // Base fare: €19.50
        // 200 loyalty points = €2.00 (cap €3.90 — under cap)
        // Fare after loyalty: €19.50 - €2.00 = €17.50
        // 15% code on €17.50: €17.50 × 0.15 = €2.625 → €2.63
        // Fare after code: €17.50 - €2.63 = €14.87
        // VAT: €14.87 × 0.21 = €3.1227 → €3.12
        // Gross: €14.87 + €3.12 = €17.99
        var code = new DiscountCode("SUMMER24", DiscountType.Percentage, 15.00m, 10.00m,
            DateTimeOffset.UtcNow.AddDays(30));

        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 200,
            code:            code
        );

        Assert.Equal(14.87m, result.NetAmount);
        Assert.Equal(3.12m,  result.VatAmount);
        Assert.Equal(17.99m, result.TotalGross);
        Assert.Equal(2.00m,  result.LoyaltyDiscountApplied);
        Assert.Equal(2.63m,  result.CodeDiscountApplied);
        Assert.Equal(200,    result.PointsUsed);
    }

    [Fact]
    public void LuxuryRide_Nighttime_WithLoyalty_AllMultipliersCombined()
    {
        // Base fare: €19.50 × 2.2 = €42.90
        // Night surcharge: €42.90 × 1.15 = €49.335 → €49.34
        // 300 points = €3.00 (cap €9.868 — under cap)
        // Fare after loyalty: €49.34 - €3.00 = €46.34
        // VAT: €46.34 × 0.21 = €9.7314 → €9.73
        // Gross: €46.34 + €9.73 = €56.07
        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Luxury,
            requestTime:     new DateTime(2025, 6, 15, 23, 0, 0),
            availablePoints: 300,
            code:            null
        );

        Assert.Equal(46.34m, result.NetAmount);
        Assert.Equal(9.73m,  result.VatAmount);
        Assert.Equal(56.07m, result.TotalGross);
        Assert.Equal(3.00m,  result.LoyaltyDiscountApplied);
        Assert.Equal(300,    result.PointsUsed);
    }

    [Fact]
    public void StandardRide_Night_LoyaltyAndCode_AllThreeCombined()
    {
        // Base: €19.50 × 1.15 night = €22.425 → €22.43
        // 200 pts loyalty = €2.00 (cap €4.486 — under cap)
        // After loyalty: €20.43
        // 15% code: €20.43 × 0.15 = €3.0645 → €3.06
        // After code: €17.37
        // VAT: €17.37 × 0.21 = €3.6477 → €3.65
        // Gross: €17.37 + €3.65 = €21.02
        var code = new DiscountCode("SUMMER24", DiscountType.Percentage, 15.00m, 10.00m,
            DateTimeOffset.UtcNow.AddDays(30));

        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 23, 0, 0),
            availablePoints: 200,
            code:            code
        );

        Assert.Equal(17.37m, result.NetAmount);
        Assert.Equal(3.65m,  result.VatAmount);
        Assert.Equal(21.02m, result.TotalGross);
        Assert.Equal(2.00m,  result.LoyaltyDiscountApplied);
        Assert.Equal(3.06m,  result.CodeDiscountApplied);
    }

    [Fact]
    public void PromoCode_Expired_IsNotApplied()
    {
        var code = new DiscountCode("EXPIRED10", DiscountType.Flat, 5.00m, 10.00m,
            DateTimeOffset.UtcNow.AddDays(-1));

        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            code
        );

        Assert.Equal(19.50m, result.NetAmount);
        Assert.Equal(0m,     result.CodeDiscountApplied);
    }

    [Fact]
    public void PromoCode_Inactive_IsNotApplied()
    {
        var code = new DiscountCode("INACTIVE", DiscountType.Flat, 5.00m, 10.00m,
            DateTimeOffset.UtcNow.AddDays(30), isActive: false);

        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            code
        );

        Assert.Equal(19.50m, result.NetAmount);
        Assert.Equal(0m,     result.CodeDiscountApplied);
    }

    [Fact]
    public void PromoCode_FareBelowMinimumRideValue_DoesNotApplyDiscount()
    {
        // Base fare: €2.50 + (1km × €1.10) + (2min × €0.30) = €4.20 → €5.00 minimum
        // Minimum ride value for code is €10.00 — €5.00 is below it
        var code = new DiscountCode("WELCOME5", DiscountType.Flat, 5.00m, 10.00m,
            DateTimeOffset.UtcNow.AddDays(30));

        var result = _engine.CalculateFinalPrice(
            distanceKm:      1.0,
            durationMinutes: 2,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            code
        );

        Assert.Equal(0m,    result.CodeDiscountApplied);
        Assert.Equal(5.00m, result.NetAmount);
    }

    [Fact]
    public void PercentagePromoCode_FareBelowMinimum_IsNotApplied()
    {
        var code = new DiscountCode("TESTCODE", DiscountType.Percentage, 15.00m, 10.00m,
            DateTimeOffset.UtcNow.AddDays(30));

        var result = _engine.CalculateFinalPrice(
            distanceKm:      1.0,
            durationMinutes: 2,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            code
        );

        Assert.Equal(0m,    result.CodeDiscountApplied);
        Assert.Equal(5.00m, result.NetAmount);
    }

    [Fact]
    public void FlatPromoCode_LargerThanFare_CappedAtFareAmount()
    {
        // Flat €999 on €19.50 fare — capped at €19.50
        // Result would be €0 → minimum fare €5.00 enforced
        var code = new DiscountCode("BIGDISCOUNT", DiscountType.Flat, 999.00m, 0.00m,
            DateTimeOffset.UtcNow.AddDays(30));

        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            code
        );

        Assert.Equal(5.00m, result.NetAmount);
        Assert.True(result.CodeDiscountApplied <= 19.50m);
        Assert.True(result.TotalGross >= 0m);
    }

    [Fact]
    public void StandardRide_FareAfterDiscountsBelowMinimum_EnforcesMinimumFare()
    {
        // Base fare: €2.50 + (0.5km × €1.10) + (2min × €0.30) = €3.65 → €5.00 minimum
        // VAT: €5.00 × 0.21 = €1.05
        // Gross: €6.05
        var result = _engine.CalculateFinalPrice(
            distanceKm:      0.5,
            durationMinutes: 2,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 0,
            code:            null
        );

        Assert.Equal(5.00m, result.NetAmount);
        Assert.Equal(1.05m, result.VatAmount);
        Assert.Equal(6.05m, result.TotalGross);
    }

    [Fact]
    public void StandardRide_LargeDiscounts_NeverProducesNegativeAmount()
    {
        // Maximum loyalty + large flat code — result must stay at minimum fare
        var code = new DiscountCode("FLAT999", DiscountType.Flat, 999.00m, 10.00m,
            DateTimeOffset.UtcNow.AddDays(30));

        var result = _engine.CalculateFinalPrice(
            distanceKm:      10.0,
            durationMinutes: 20,
            vehicleType:     VehicleType.Standard,
            requestTime:     new DateTime(2025, 6, 15, 12, 0, 0),
            availablePoints: 10000,
            code:            code
        );

        Assert.True(result.NetAmount  >= 0m);
        Assert.True(result.TotalGross >= 0m);
        Assert.Equal(5.00m, result.NetAmount);
    }

    [Fact]
    public void CalculateEarnedPoints_ReturnsCorrectPoints()
    {
        // €23.60 × 10 = 236 points
        var points = _engine.CalculateEarnedPoints(23.60m);
        Assert.Equal(236, points);
    }

    [Fact]
    public void CalculateEarnedPoints_TruncatesFractionalPoints()
    {
        // €19.99 × 10 = 199.9 → truncated to 199
        var points = _engine.CalculateEarnedPoints(19.99m);
        Assert.Equal(199, points);
    }

    [Fact]
    public void CalculateEarnedPoints_ZeroAmount_ReturnsZeroPoints()
    {
        var points = _engine.CalculateEarnedPoints(0m);
        Assert.Equal(0, points);
    }
}