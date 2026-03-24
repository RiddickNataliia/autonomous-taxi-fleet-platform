namespace NovaDrive.Tests.UnitTests;

public class DiscountCodeTests
{
    //helpers:
    private static DiscountCode CreateValidPercentageCode() =>
        new DiscountCode(
            code: "PERC15",
            type: DiscountType.Percentage,
            value: 15m,
            minimumRideValue: 10m,
            expirationDate: DateTimeOffset.UtcNow.AddDays(1),
            isActive: true
        );

    private static DiscountCode CreateValidFlatCode() =>
        new DiscountCode(
            code: "FLAT5",
            type: DiscountType.Flat,
            value: 5m,
            minimumRideValue: 20m,
            expirationDate: DateTimeOffset.UtcNow.AddDays(1),
            isActive: true
        );

    private static DiscountCode CreateExpiredCode() =>
        new DiscountCode(
            code: "EXPIRED",
            type: DiscountType.Percentage,
            value: 10m,
            minimumRideValue: 0m,
            expirationDate: DateTimeOffset.UtcNow.AddDays(-1),
            isActive: true
        );

    private static DiscountCode CreateInactiveCode() =>
        new DiscountCode(
            code: "INACTIVE",
            type: DiscountType.Flat,
            value: 5m,
            minimumRideValue: 0m,
            expirationDate: DateTimeOffset.UtcNow.AddDays(1),
            isActive: false
        );

    [Fact]
    public void IsValid_ReturnsTrue_ForActiveUnexpiredAboveMinimum()
    {
        var code = CreateValidPercentageCode();
        Assert.True(code.IsValid(15m));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForExpiredCode()
    {
        var code = CreateExpiredCode();
        Assert.False(code.IsValid(15m));
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForInactiveCode()
    {
        var code = CreateInactiveCode();
        Assert.False(code.IsValid(15m));
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenFareBelowMinimum()
    {
        var code = CreateValidFlatCode();
        Assert.False(code.IsValid(15m)); // minimum is 20
    }

    [Fact]
    public void CalculateDiscount_PercentageType_CalculatesCorrectly()
    {
        var code = CreateValidPercentageCode();
        decimal discount = code.CalculateDiscount(100m);
        Assert.Equal(15m, discount); // 15% of 100
    }

    [Fact]
    public void CalculateDiscount_FlatType_CalculatesCorrectly()
    {
        var code = CreateValidFlatCode();
        decimal discount = code.CalculateDiscount(50m);
        Assert.Equal(5m, discount); // flat €5
    }

    [Fact]
    public void CalculateDiscount_FlatType_CappedAtFareAmount()
    {
        var code = CreateValidFlatCode();
        decimal discount = code.CalculateDiscount(4m);
        Assert.Equal(4m, discount); // cannot exceed fare amount
    }

    [Fact]
    public void CalculateDiscount_UnknownType_ReturnsZero()
    {
        var code = new DiscountCode(
            code: "UNKNOWN",
            type: DiscountType.Unknown,
            value: 0m,
            minimumRideValue: 0m,
            expirationDate: DateTimeOffset.UtcNow.AddDays(1),
            isActive: true
        );
        decimal discount = code.CalculateDiscount(100m);
        Assert.Equal(0m, discount);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var code = CreateValidPercentageCode();
        code.Deactivate();
        Assert.False(code.IsActive);
    }

    [Fact]
    public void Deactivate_IsIdempotent()
    {
        var code = CreateValidPercentageCode();
        code.Deactivate();
        code.Deactivate(); // should not throw or change state further
        Assert.False(code.IsActive);
    }

    [Fact]
    public void Create_WithEmptyCode_Throws() =>
        Assert.Throws<DiscountDomainException>(() =>
            DiscountCode.Create("", DiscountType.Percentage, 10m, 0m, DateTimeOffset.UtcNow.AddDays(1)));

    [Fact]
    public void Create_WithUnknownType_Throws() =>
        Assert.Throws<DiscountDomainException>(() =>
            DiscountCode.Create("CODE", DiscountType.Unknown, 10m, 0m, DateTimeOffset.UtcNow.AddDays(1)));

    [Fact]
    public void Create_WithNegativeValue_Throws() =>
        Assert.Throws<DiscountDomainException>(() =>
            DiscountCode.Create("CODE", DiscountType.Flat, -5m, 0m, DateTimeOffset.UtcNow.AddDays(1)));

    [Fact]
    public void Create_WithPastExpirationDate_Throws() =>
        Assert.Throws<DiscountDomainException>(() =>
            DiscountCode.Create("CODE", DiscountType.Flat, 5m, 0m, DateTimeOffset.UtcNow.AddDays(-1))); 

}