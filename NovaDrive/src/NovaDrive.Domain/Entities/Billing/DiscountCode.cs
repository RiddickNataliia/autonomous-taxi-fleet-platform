namespace NovaDrive.Domain.Entities;

public class DiscountCode
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Code { get; init; } 
    public DiscountType Type { get; init; } = DiscountType.Unknown;
    public decimal Value { get; init; } // 15 for 15% or 5 for €5.00
    public DateTimeOffset ExpirationDate { get; init; }
    public decimal MinimumRideValue { get; init; }
    public bool IsActive { get; private set; } = true;

    private DiscountCode() { }

    /// <summary>
    /// Checks if the discount code can be applied. 
    /// Requirement: Current date < Expiration AND Fare >= MinimumValue.
    /// </summary>
    public bool IsValid(decimal rideAmount)
    {
        return IsActive && 
               DateTimeOffset.UtcNow <= ExpirationDate && 
               rideAmount >= MinimumRideValue;
    }

    /// <summary>
    /// Logic for Rule: "Discount Codes" applied AFTER loyalty discount.
    /// </summary>
    public decimal CalculateDiscount(decimal currentFare)
    {
        if (!IsValid(currentFare)) return 0m;

        if (Type == DiscountType.Percentage)
            return Math.Round(currentFare * (Value / 100m), 2);

        if (Type == DiscountType.Flat)
            return Math.Min(Value, currentFare); // Ensure we don't discount more than the fare

        return 0m;
    }

    public void Deactivate() => IsActive = false;
}