namespace NovaDrive.Domain.Entities;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.Unknown;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }

    // Navigation property — EF Core populates this on explicit Include()
    // Null for admin and vehicle-system users who have no passenger profile
    public Passenger? PassengerProfile { get; private set; }

    // Private constructor for EF Core
    private User() { }

    /// <summary>
    /// Factory method for creating any user. Use Passenger.Create() for passenger accounts.
    /// </summary>
    public static User Create(string email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new UserDomainException(UserDomainException.InvalidEmail);

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new UserDomainException(UserDomainException.InvalidPassword);

        return new User
        {
            Email = email,
            PasswordHash = passwordHash,
            Role = role
        };
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }
}