using System.Reflection.Metadata.Ecma335;

namespace NovaDrive.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Vin VIN { get; init; }
    public int YearOfManufacture { get; init; }
    public required string LicensePlate { get; init; }
    public required string ModelName { get; set; }
    public VehicleType Type { get; set; } = VehicleType.Unknown;
    
    //Vitals and status
    public GpsLocation CurrentLocation { get; private set; } = new(0, 0);
    public BatteryLevel Battery { get; private set; }
    public VehicleStatus Status { get; private set; } = VehicleStatus.Inactive;
    public DateTimeOffset? LastInspectionDate { get; private set; }
    public string? ApiKeyHash { get; private set; }

    private Vehicle() { }

    /// <summary>
    /// Activates the vehicle for use in the fleet.
    /// </summary>
    /// <exception cref="VehicleDomainException">Thrown if the vehicle is overdue for an inspection.</exception>
    public void Activate() 
    {
        if (Status == VehicleStatus.Active) return;

        if (NeedsInspection())
            throw new VehicleDomainException(VehicleDomainException.InspectionRequired);
        
        if (Battery.IsLow)
            throw new VehicleDomainException(VehicleDomainException.LowBattery);

        Status = VehicleStatus.Active;
    }

    /// <summary>
    /// Temporarily takes the vehicle out of service.
    /// </summary>
    public void Deactivate()
    {
        if (Status == VehicleStatus.Inactive) return;
        Status = VehicleStatus.Inactive;
    }

    /// <summary>
    /// Manually marks vehicle for maintenance
    /// </summary>
    public void MarkForMaintenance()
    {
        if (Status == VehicleStatus.Maintenance) return;
        Status = VehicleStatus.Maintenance;
    }

    /// <summary>
    /// Records a successful safety inspection, resetting the 12-month inspection timer.
    /// </summary>
    public void RecordInspection() => LastInspectionDate = DateTimeOffset.UtcNow;

    /// <summary>
    /// Determines if a safety inspection is required. 
    /// Inspection is due if 12 months have passed since the last one or if a vehicle has never been inspected.
    /// </summary>
    /// <returns>True if the vehicle requires an inspection; otherwise, false.</returns>
    public bool NeedsInspection()
    {
        if (LastInspectionDate == null) return true;
        return LastInspectionDate.Value.AddYears(1) < DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Assigns a new API key to this vehicle.
    /// </summary>
    /// <param name="hashedKey">The BCrypt hash of the generated API key.</param>
    public void SetApiKey(string hashedKey)
    {
        if (string.IsNullOrWhiteSpace(hashedKey))
            throw new VehicleDomainException(VehicleDomainException.InvalidApiKey);

        if (!hashedKey.StartsWith("$2") || hashedKey.Length != 60)
            throw new VehicleDomainException(VehicleDomainException.InvalidApiKey);

        if (Status == VehicleStatus.EnRoute)
            throw new VehicleDomainException(VehicleDomainException.CannotRotateKeyWhileEnRoute);

        if (ApiKeyHash == hashedKey) return;

        ApiKeyHash = hashedKey;
    }

    /// <summary>
    /// Keeps track of vehicle's locatoin, battery and status 
    /// </summary>
    public void UpdateVitals(GpsLocation newLocation, BatteryLevel newBattery)
    {
        CurrentLocation = newLocation;
        Battery = newBattery;

        if (Battery.IsCritical)
        {
            if (Status == VehicleStatus.EnRoute)
                Status = VehicleStatus.Maintenance; // stranded mid-ride (maybe I'll add sending a replacement later)
            else if (Status == VehicleStatus.Active)
                Status = VehicleStatus.Inactive;
        }
    }
}