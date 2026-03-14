namespace NovaDrive.Domain.Entities;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.Unknown;
    
    // Loyalty Program Requirements
    public int LoyaltyPoints { get; private set; } // Protected from direct tampering
    
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }


    private User() { }

    /// <summary>
    /// Adds points to the passenger's balance (e.g., after a successful payment).
    /// </summary>
    public void EarnPoints(int points)
    {
        if (points < 0) 
            throw new UserDomainException(UserDomainException.NegativeEarn);

        LoyaltyPoints += points;
    }


    /// <summary>
    /// Deducts points used for a discount.
    /// </summary>
    public void DeductPoints(int points)
    {
        if (points < 0) 
            throw new UserDomainException(UserDomainException.NegativeDecuct);
        
        if (points > LoyaltyPoints)
            throw new UserDomainException(UserDomainException.InsufficientPoints);

        LoyaltyPoints -= points;
    }
}