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
    /// Checks whether this code can be applied to the given fare.
    /// The fare passed in must be the amount after loyalty discount has already been deducted.
    /// </summary>
    /// <param name="fareBeforeCodeDiscount">The total fare amount after loyalty discount</param>
    public bool IsValid(decimal fareBeforeCodeDiscount)
    {
        return IsActive && 
               DateTimeOffset.UtcNow <= ExpirationDate && 
               fareBeforeCodeDiscount >= MinimumRideValue;
    }

    /// <summary>
    /// Calculates the discount amount to subtract from the fare.
    /// Must only be called after the loyalty discount has been applied and IsValid() has returned true.
    /// </summary>
    /// <param name="fareBeforeCodeDiscount">The total fare amount after loyalty discount</param>
    public decimal CalculateDiscount(decimal fareBeforeCodeDiscount)
    {
        return Type switch
        {
            DiscountType.Percentage => Math.Round(fareBeforeCodeDiscount * (Value / 100m), 2),
            DiscountType.Flat       => Math.Min(Value, fareBeforeCodeDiscount),
            _                       => 0m
        };
    }
    /// <summary>
    /// Deactivates a discount code.
    /// </summary>
    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }
}