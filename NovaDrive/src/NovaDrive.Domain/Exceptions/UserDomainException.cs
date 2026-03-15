namespace NovaDrive.Domain.Exceptions;

public class UserDomainException : Exception
{
    public const string NegativeDeduct = "Cannot deduct a negative amount of points.";
    public const string NegativeEarn = "Cannot earn a negative amount of points.";
    public const string InsufficientPoints = "Insufficient loyalty points balance.";
    public const string InvalidEmail = "A valid email address is required.";
    public const string InvalidPassword = "A password hash is required.";
    public const string InvalidFullName = "A full name is required.";
    public const string InvalidUserId = "A valid user ID is required to create a passenger profile.";

    public UserDomainException(string message) : base(message) { }
}