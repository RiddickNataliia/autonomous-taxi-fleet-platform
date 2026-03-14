namespace NovaDrive.Domain.Entities;

public class DiscountCode
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Code { get; init; } 
    public DiscountType Type { get; init; } = DiscountType.Unknown;
    public decimal Value { get; init; }
    public DateTimeOffset ExpirationDate { get; init; }
    public decimal MinimumRideValue { get; init; }
    public bool IsActive { get; private set; } = true;


    private DiscountCode() { }

    /// <summary>
    /// Checks if the discount code can be applied to a specific ride amount.
    /// </summary>
    public bool IsValid(decimal rideAmount)
    {
        return IsActive && 
               DateTimeOffset.UtcNow <= ExpirationDate && 
               rideAmount >= MinimumRideValue;
    }

    /// <summary>
    /// Calculates the savings based on the ride amount.
    /// </summary>
    public decimal CalculateDiscount(decimal currentFare)
    {
        // The requirement says: "gives a 15% discount on the REMAINING amount"
        // So we pass the fare AFTER loyalty discount into this method.
        
        if (Type == DiscountType.Percentage)
            return Math.Round(currentFare * (Value / 100m), 2);

        return Math.Min(Value, currentFare);
    }

    /// <summary>
    /// Deactivate a discount code.
    /// </summary>
    public void Deactivate() => IsActive = false;
}