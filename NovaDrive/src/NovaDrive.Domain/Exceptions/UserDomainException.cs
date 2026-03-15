namespace NovaDrive.Domain.Exceptions;

public class UserDomainException : DomainException
{   
    public const string NegativeDecuct = "Cannot deduct a negative amount of points.";
    public const string NegativeEarn = "Cannot earn a negative amount of points.";
    public const string InsufficientPoints = "Insufficient loyalty points balance.";
    public UserDomainException(string message) : base(message) { }
}

