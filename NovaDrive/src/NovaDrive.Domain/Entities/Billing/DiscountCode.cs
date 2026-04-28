namespace NovaDrive.Domain.Entities;

public class DiscountCode
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Code { get; init; } = string.Empty;
    public DiscountType Type { get; init; } = DiscountType.Unknown;
    public decimal Value { get; init; } // 15 for 15% or 5 for €5.00
    public DateTimeOffset ExpirationDate { get; private set; }
    public decimal MinimumRideValue { get; init; }
    public bool IsActive { get; private set; } = true;

    //private to enforce creation through the factory method with validation
    private DiscountCode() { }

    //for testing purposes only - allows setting all properties including those with private setters
    internal DiscountCode(string code, DiscountType type, decimal value, decimal minimumRideValue, DateTimeOffset expirationDate, bool isActive = true)
    {
        Code = code;
        Type = type;
        Value = value;
        MinimumRideValue = minimumRideValue;
        ExpirationDate = expirationDate;
        IsActive = isActive;
    }

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
            DiscountType.Percentage => Math.Round(fareBeforeCodeDiscount * (Value / 100m), 2, MidpointRounding.AwayFromZero),
            DiscountType.Flat       => Math.Min(Value, fareBeforeCodeDiscount),
            _                       => 0m
        };
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    public void Activate(DateTimeOffset newExpirationDate)
    {
        if (newExpirationDate <= DateTimeOffset.UtcNow)
            throw new DiscountDomainException(DiscountDomainException.AlreadyExpired);

        IsActive = true;
        ExpirationDate = newExpirationDate;
    }


    /// <summary>
    /// Factory method to create a new discount code with validation. Throws exceptions if any parameters are invalid.
    /// </summary>
    /// <exception cref="DiscountDomainException"></exception>
    public static DiscountCode Create(string code, DiscountType type, decimal value, decimal minimumRideValue, DateTimeOffset expirationDate)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DiscountDomainException(DiscountDomainException.InvalidCode);

        if (type == DiscountType.Unknown)
            throw new DiscountDomainException(DiscountDomainException.InvalidType);

        if (value <= 0)
            throw new DiscountDomainException(DiscountDomainException.InvalidValue);

        if (expirationDate <= DateTimeOffset.UtcNow)
            throw new DiscountDomainException(DiscountDomainException.AlreadyExpired);

        return new DiscountCode
        {
            Code = code,
            Type = type,
            Value = value,
            MinimumRideValue = minimumRideValue,
            ExpirationDate = expirationDate
        };
    }

    
}