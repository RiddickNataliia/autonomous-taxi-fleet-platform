namespace NovaDrive.Domain.Exceptions;

public class VehicleDomainException : Exception
{
    public const string InspectionRequired = "Cannot activate a vehicle that is overdue for inspection.";
    public const string LowBattery = "Cannot activate vehicle with low battery (under 10%).";
    public const string InvalidVin = "A valid VIN is required.";

    public VehicleDomainException(string message) : base(message) { }
}