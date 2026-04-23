namespace NovaDrive.Domain.Exceptions;

public class UserDomainException : Exception
{
    public const string NotFound = "User not found.";
    public const string PassengerNotFound = "Passenger profile not found.";
    public const string NegativeDeduct = "Cannot deduct a negative amount of points.";
    public const string NegativeEarn = "Cannot earn a negative amount of points.";
    public const string InsufficientPoints = "Insufficient loyalty points balance.";
    public const string InvalidEmail = "A valid email address is required.";
    public const string EmailAlreadyInUse = "An account with this email address already exists.";
    public const string InvalidAuth0UserId = "A valid Auth0 user ID is required.";
    public const string InvalidFullName = "A full name is required.";
    public const string InvalidUserId = "A valid user ID is required to create a passenger profile.";
    public const string InvalidRole = "A valid role must be assigned to a user.";
    public const string InvalidHomeAddress = "A valid home address is required.";
    public const string InvalidPaymentMethod = "A valid payment method is required.";

    public UserDomainException(string message) : base(message) { }
}