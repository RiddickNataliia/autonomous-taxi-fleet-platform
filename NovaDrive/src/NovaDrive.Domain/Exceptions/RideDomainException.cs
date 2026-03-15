namespace NovaDrive.Domain.Exceptions;

public class RideDomainException : DomainException
{
    public const string AlreadyInitialized = "Ride has already been initialized.";
    public const string NotRequested = "Only requested rides can be started.";
    public const string NotEnRoute = "Only rides that are En Route can be completed.";
    public const string AlreadyCompleted = "Cannot cancel a ride that is already completed.";
    public const string NotCompleted = "Only completed rides can be marked as paid.";
    public RideDomainException(string message) : base(message)
    {
    }
}