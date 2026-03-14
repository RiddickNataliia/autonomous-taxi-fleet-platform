namespace NovaDrive.Domain.Entities;

public class Vehicle
{
public Guid Id { get; init; } = Guid.NewGuid();
    public string VIN { get; init; } = string.Empty;
    public int YearOfManufacture { get; init; }
    public required string LicensePlate { get; init; }
    public required string ModelName { get; set; }
    public VehicleType Type { get; set; } = VehicleType.Unknown;
    
    //Vitals and status
    public GpsLocation CurrentLocation { get; private set; } = new(0, 0);
    public int BatteryPercentage { get; private set; }
    public VehicleStatus Status { get; private set; } = VehicleStatus.Inactive;
    public DateTimeOffset? LastInspectionDate { get; private set; }

    private Vehicle() { }

    /// <summary>
    /// Activates the vehicle for use in the fleet.
    /// </summary>
    /// <exception cref="VehicleDomainException">Thrown if the vehicle is overdue for an inspection.</exception>
    public void Activate() 
    {
        if (NeedsInspection())
            throw new VehicleDomainException("Cannot activate a vehicle that is overdue for inspection.");
        
        if (BatteryPercentage < 10)
            throw new VehicleDomainException("Cannot activate vehicle with low battery (under 10%).");

        Status = VehicleStatus.Active;
    }

    /// <summary>
    /// Temporarily takes the vehicle out of service (e.g., for maintenance or repair).
    /// </summary>
    public void Deactivate() => Status = VehicleStatus.Inactive;

    /// <summary>
    /// Manually marks vehicle for maintenance
    /// </summary>
    public void MarkForMaintenance() => Status = VehicleStatus.Maintenance;

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
    /// Keeps track of vehicle's locatoin, battery and status 
    /// </summary>
    /// <param name="newLocation"></param>
    /// <param name="newBattery"></param>
    public void UpdateVitals(GpsLocation newLocation, int newBattery)
    {
        CurrentLocation = newLocation;
        BatteryPercentage = Math.Clamp(newBattery, 0, 100);

        if (BatteryPercentage <= 0)
        {
            if (Status == VehicleStatus.EnRoute)
            {
                // This status tells the system: "Send help to this specific location!"
                Status = VehicleStatus.Maintenance; 
                
                // could trigger a 'RideFailedEvent'nso the Dispatcher knows to send a second car for the passenger.
            }
            else if (Status == VehicleStatus.Active)
            {
                Status = VehicleStatus.Inactive;
            }
        }
    }
}