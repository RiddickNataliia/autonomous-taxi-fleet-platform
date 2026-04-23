namespace NovaDrive.Domain.Entities;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Email { get; set; }
    public string Auth0UserId { get; private set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Unknown;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastLoginAt { get; set; }

    // Navigation property
    // Null for admin and vehicle-system users who have no passenger profile
    public Passenger? PassengerProfile { get; private set; }

    // Private constructor for EF Core
    private User() { }

    /// <summary>
    /// Factory method for creating any user. Use Passenger.Create() for passenger accounts.
    /// </summary>
    public static User Create(string email, string auth0UserId, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new UserDomainException(UserDomainException.InvalidEmail);

        if (string.IsNullOrWhiteSpace(auth0UserId))
            throw new UserDomainException(UserDomainException.InvalidAuth0UserId);

        if (role == UserRole.Unknown)
            throw new UserDomainException(UserDomainException.InvalidRole);

        return new User
        {
            Email      = email,
            Auth0UserId = auth0UserId,
            Role       = role
        };
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
    }
}