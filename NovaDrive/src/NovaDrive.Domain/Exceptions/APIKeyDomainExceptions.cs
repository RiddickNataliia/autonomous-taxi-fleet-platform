namespace NovaDrive.Domain.Exceptions;
public class APIKeyDomainException : DomainException
{
    public const string InvalidApiKey = "A valid hashed API key is required.";
    public const string CannotRotateKeyWhileEnRoute = "Cannot rotate API key while vehicle is on an active ride.";
    public APIKeyDomainException(string message) : base(message) { }
}