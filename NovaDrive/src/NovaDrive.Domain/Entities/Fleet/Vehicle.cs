namespace NovaDrive.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Vin VIN { get; init; } = default!;
    public int YearOfManufacture { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public string ModelName { get; init; } = string.Empty;
    public VehicleType Type { get; init; } = VehicleType.Unknown;
    
    //Vitals and status
    public GpsLocation CurrentLocation { get; private set; } = new(0, 0);
    public BatteryLevel Battery { get; private set; } = new(100);
    public VehicleStatus Status { get; private set; } = VehicleStatus.Inactive;
    public DateTimeOffset? LastInspectionDate { get; private set; }
    public string? ApiKeyHash { get; private set; }

    private Vehicle() { }

    //for testing to avoid creation though the factory method with validation.
    //allows setting all properties including those with private setters
    internal Vehicle(string licensePlate, string modelName, Vin vin, int yearOfManufacture = 2023, VehicleType type = VehicleType.Standard, VehicleStatus status = VehicleStatus.Inactive, GpsLocation? currentLocation = null, BatteryLevel? battery = null)
    {
        LicensePlate      = licensePlate;
        ModelName         = modelName;
        VIN               = vin;
        YearOfManufacture = yearOfManufacture;
        Type              = type;
        Status            = status;
        CurrentLocation   = currentLocation ?? new GpsLocation(0, 0);
        Battery           = battery ?? new BatteryLevel(100);
    }

    public static Vehicle Create(string licensePlate, string modelName, Vin vin, int yearOfManufacture, VehicleType type)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            throw new VehicleDomainException(VehicleDomainException.InvalidLicensePlate);

        if (string.IsNullOrWhiteSpace(modelName))
            throw new VehicleDomainException(VehicleDomainException.InvalidModelName);

        if (type == VehicleType.Unknown)
            throw new VehicleDomainException(VehicleDomainException.InvalidType);

        return new Vehicle
        {
            VIN               = vin,
            LicensePlate      = licensePlate,
            ModelName         = modelName,
            YearOfManufacture = yearOfManufacture,
            Type              = type
        };
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

    /// <summary>
    /// Removes the vehicle's API key hash, immediately preventing the vehicle
    /// simulator from authenticating. Used when a key is compromised or the
    /// vehicle is decommissioned. A new key can be provisioned afterwards.
    /// </summary>
    public void ClearApiKey()
    {
        if (Status == VehicleStatus.EnRoute)
            throw new VehicleDomainException(VehicleDomainException.CannotRotateKeyWhileEnRoute);
        ApiKeyHash = null;
    }

}