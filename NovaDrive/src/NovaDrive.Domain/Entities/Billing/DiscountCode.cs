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
    public decimal CalculateDiscount(decimal fareBeforeThisDiscount)
    {
        if (!IsActive || DateTimeOffset.UtcNow > ExpirationDate)
            return 0m;

        if (fareBeforeThisDiscount < MinimumRideValue)
            return 0m;

        return Type switch
        {
            DiscountType.Percentage => Math.Round(fareBeforeThisDiscount * (Value / 100m), 2),
            DiscountType.Flat       => Math.Min(Value, fareBeforeThisDiscount),
            _                       => 0m
        };
    }

    public void Deactivate() => IsActive = false;
}