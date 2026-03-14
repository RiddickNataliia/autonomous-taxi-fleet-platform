namespace NovaDrive.Domain.Services;

/// <summary>
/// A data transfer record that holds the result of the pricing calculation.
/// </summary>
public record PricingResult(
    decimal NetAmount, 
    decimal VatAmount, 
    decimal TotalGross, 
    decimal LoyaltyDiscountApplied, 
    int PointsUsed);