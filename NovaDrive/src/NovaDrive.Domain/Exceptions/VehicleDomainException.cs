namespace NovaDrive.Domain.Exceptions;

public class VehicleDomainException : Exception
{
    public const string InspectionRequired = "Cannot activate a vehicle that is overdue for inspection.";
    public const string LowBattery = "Cannot activate vehicle with low battery (under 10%).";
    public const string InvalidVin = "A valid VIN is required.";
    public const string InvalidApiKey = "A valid hashed API key is required.";
    public const string CannotRotateKeyWhileEnRoute = "Cannot rotate API key while vehicle is on an active ride.";

    public VehicleDomainException(string message) : base(message) { }
}