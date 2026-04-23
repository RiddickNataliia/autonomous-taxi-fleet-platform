namespace NovaDrive.Domain.Exceptions;

public class VehicleDomainException : Exception
{
    public const string NotFound = "Vehicle not found.";
    public const string InspectionRequired = "Cannot activate a vehicle that is overdue for inspection.";
    public const string LowBattery = "Cannot activate vehicle with low battery (under 10%).";
    public const string InvalidVin = "A valid VIN is required.";
    public const string VinAlreadyRegistered = "A vehicle with this VIN is already registered.";
    public const string LicensePlateAlreadyInUse = "A vehicle with this license plate is already registered.";
    public const string InvalidApiKey = "A valid hashed API key is required.";
     public const string NoActiveApiKey = "This vehicle has no active API key. Provision one first.";
    public const string CannotRotateKeyWhileEnRoute = "Cannot rotate API key while vehicle is on an active ride.";

    public const string InvalidLicensePlate = "A valid license plate is required.";
    public const string InvalidModelName    = "A valid model name is required.";
    public const string InvalidType         = "A valid vehicle type is required.";

    public VehicleDomainException(string message) : base(message) { }
}