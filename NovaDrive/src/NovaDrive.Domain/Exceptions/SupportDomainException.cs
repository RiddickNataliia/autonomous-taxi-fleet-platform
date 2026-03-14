namespace NovaDrive.Domain.Exceptions;

public class SupportDomainException : Exception
{
    public const string NotOpen = "Only open tickets can be started.";
    public const string AlreadyResolved = "This ticket is already resolved.";
    public const string InvalidSubject = "A valid subject is required for the support ticket.";

    public SupportDomainException(string message) : base(message) { }
}