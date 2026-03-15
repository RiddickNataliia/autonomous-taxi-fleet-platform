namespace NovaDrive.Domain.Exceptions;

public class MaintenanceDomainException : Exception
{
    public const string NegativeCost = "Maintenance cost cannot be negative.";
    public const string MissingDescription = "A description is required for a maintenance log.";
    public const string MissingTechnician = "A technician name is required for a maintenance log.";

    public MaintenanceDomainException(string message) : base(message) { }
}