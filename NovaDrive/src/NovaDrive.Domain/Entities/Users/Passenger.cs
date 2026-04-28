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


    public static Passenger Create(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new UserDomainException(UserDomainException.InvalidUserId);

        return new Passenger
        {
            UserId = userId
        };
    }


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

    public void EarnPoints(int points)
    {
        if (points < 0)
            throw new UserDomainException(UserDomainException.NegativeEarn);

        if (points == 0) return;

        LoyaltyPoints += points;
    }


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