namespace NovaDrive.Domain.Exceptions;

public class SupportDomainException : DomainException
{
    public const string NotOpen = "Only open tickets can be started.";
    public const string AlreadyResolved = "This ticket is already resolved.";
    public const string InvalidSubject = "A valid subject is required for the support ticket.";
    public const string InvalidDescription = "A description is required for the support ticket.";
    public const string InvalidPriority = "A valid priority must be provided.";
    public const string NotInProgress = "Only tickets in progress can be resolved.";

    public SupportDomainException(string message) : base(message) { }
}