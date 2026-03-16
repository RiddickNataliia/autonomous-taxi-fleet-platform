namespace NovaDrive.Domain.Entities;

public class Passenger
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required Guid UserId { get; init; }
    public User? User { get; private set; }

    public string FullName { get; private set; } = string.Empty;
    public string HomeAddress { get; private set; } = string.Empty;
    public PaymentMethod PreferredPaymentMethod { get; private set; } = PaymentMethod.Unknown;
    public int LoyaltyPoints { get; private set; }


    private Passenger() { }

    /// <summary>
    /// Factory method — creates a passenger profile linked to an existing User.
    /// Call this immediately after User.Create() when registering a passenger.
    /// </summary>
    public static Passenger Create(Guid userId, string fullName, string homeAddress)
    {
        if (userId == Guid.Empty)
            throw new UserDomainException(UserDomainException.InvalidUserId);

        if (string.IsNullOrWhiteSpace(fullName))
            throw new UserDomainException(UserDomainException.InvalidFullName);

        if (string.IsNullOrWhiteSpace(homeAddress))
            throw new UserDomainException(UserDomainException.InvalidHomeAddress);

        return new Passenger
        {
            UserId = userId,
            FullName = fullName,
            HomeAddress = homeAddress
        };
    }

    /// <summary>
    /// Updates editable profile fields.
    /// </summary>
    public void UpdateProfile(string fullName, string homeAddress, PaymentMethod paymentMethod)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new UserDomainException(UserDomainException.InvalidFullName);

        if (string.IsNullOrWhiteSpace(homeAddress))
            throw new UserDomainException(UserDomainException.InvalidHomeAddress);

        if (paymentMethod == PaymentMethod.Unknown)
            throw new UserDomainException(UserDomainException.InvalidPaymentMethod);

        FullName = fullName;
        HomeAddress = homeAddress;
        PreferredPaymentMethod = paymentMethod;
    }

    /// <summary>
    /// Awards loyalty points after a successful ride payment.
    /// </summary>
    public void EarnPoints(int points)
    {
        if (points < 0)
            throw new UserDomainException(UserDomainException.NegativeEarn);

        if (points == 0) return;

        LoyaltyPoints += points;
    }

    /// <summary>
    /// Deducts loyalty points consumed as a discount.
    /// Only called after PricingEngine determines the exact points spent.
    /// </summary>
    public void DeductPoints(int points)
    {
        if (points < 0)
            throw new UserDomainException(UserDomainException.NegativeDeduct);

        if (points == 0) return;

        if (points > LoyaltyPoints)
            throw new UserDomainException(UserDomainException.InsufficientPoints);

        LoyaltyPoints -= points;
    }
}