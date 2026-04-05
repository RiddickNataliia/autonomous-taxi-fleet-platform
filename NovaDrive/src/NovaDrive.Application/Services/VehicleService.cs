namespace NovaDrive.Application.Services;

public interface IVehicleService
{
    Task<VehicleResponse> RegisterVehicle(RegisterVehicleRequest request, CancellationToken ct = default);
    Task<VehicleResponse> GetVehicle(Guid vehicleId, CancellationToken ct = default);
    Task<IEnumerable<VehicleResponse>> GetActiveVehicles(CancellationToken ct = default);
    Task<VehicleResponse> ActivateVehicle(Guid vehicleId, CancellationToken ct = default);
    Task<VehicleResponse> DeactivateVehicle(Guid vehicleId, CancellationToken ct = default);
    Task UpdateVitals(UpdateVehicleVitalsRequest request, CancellationToken ct = default);
    Task<MaintenanceLogResponse> RecordInspection(Guid vehicleId, CreateLogRequest request, CancellationToken ct = default);
    Task<MaintenanceLogResponse> AddMaintenanceLog(CreateLogRequest request, CancellationToken ct = default);
    Task<IEnumerable<MaintenanceLogResponse>> GetMaintenanceHistory(Guid vehicleId, CancellationToken ct = default);
    Task<ProvisionApiKeyResponse> ProvisionApiKey(Guid vehicleId, CancellationToken ct = default);
    Task RevokeApiKey(Guid vehicleId, CancellationToken ct = default);
}
public sealed class VehicleService : IVehicleService
{
    private readonly IVehicleRepository        _vehicleRepo;
    private readonly IMaintenanceLogRepository _maintenanceRepo;
    private readonly IUnitOfWork               _unitOfWork;

    public VehicleService(
        IVehicleRepository        vehicleRepo,
        IMaintenanceLogRepository maintenanceRepo,
        IUnitOfWork               unitOfWork)
    {
        _vehicleRepo     = vehicleRepo;
        _maintenanceRepo = maintenanceRepo;
        _unitOfWork      = unitOfWork;
    }

    public async Task<VehicleResponse> RegisterVehicle(
        RegisterVehicleRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<VehicleType>(request.VehicleType, ignoreCase: true, out var type)
            || type == VehicleType.Unknown)
            throw new VehicleDomainException("Invalid vehicle type.");

        // Duplicate VIN check — before touching the DB
        var existing = await _vehicleRepo.GetByVin(request.Vin, ct);
        if (existing is not null)
            throw new VehicleDomainException(VehicleDomainException.VinAlreadyRegistered);

        var vehicle = new Vehicle
        {
            VIN               = new Vin(request.Vin),
            LicensePlate      = request.LicensePlate,
            ModelName         = request.ModelName,
            YearOfManufacture = request.YearOfManufacture,
            Type              = type
        };

        await _vehicleRepo.Add(vehicle, ct);
        await _unitOfWork.SaveChanges(ct);
        return vehicle.ToResponse();
    }

    public async Task<VehicleResponse> GetVehicle(Guid vehicleId, CancellationToken ct = default)
    {
        var vehicle = await _vehicleRepo.GetById(vehicleId, ct)
            ?? throw new KeyNotFoundException(VehicleDomainException.NotFound);
        return vehicle.ToResponse();
    }

    public async Task<IEnumerable<VehicleResponse>> GetActiveVehicles(CancellationToken ct = default)
        => (await _vehicleRepo.GetAllActive(ct)).Select(v => v.ToResponse());

    public async Task<VehicleResponse> ActivateVehicle(Guid vehicleId, CancellationToken ct = default)
    {
        var vehicle = await _vehicleRepo.GetById(vehicleId, ct)
            ?? throw new KeyNotFoundException(VehicleDomainException.NotFound);

        vehicle.Activate(); // throws VehicleDomainException if inspection due or battery low
        await _unitOfWork.SaveChanges(ct);
        return vehicle.ToResponse();
    }

    public async Task<VehicleResponse> DeactivateVehicle(Guid vehicleId, CancellationToken ct = default)
    {
        var vehicle = await _vehicleRepo.GetById(vehicleId, ct)
            ?? throw new KeyNotFoundException(VehicleDomainException.NotFound);

        vehicle.Deactivate();
        await _unitOfWork.SaveChanges(ct);
        return vehicle.ToResponse();
    }

    public async Task UpdateVitals(
        UpdateVehicleVitalsRequest request, CancellationToken ct = default)
    {
        var vehicle = await _vehicleRepo.GetById(request.VehicleId, ct)
            ?? throw new KeyNotFoundException(VehicleDomainException.NotFound);

        vehicle.UpdateVitals(
            new GpsLocation(request.Latitude, request.Longitude),
            new BatteryLevel(request.BatteryPercentage));

        await _unitOfWork.SaveChanges(ct);
    }

    /// <summary>
    /// Records an inspection: updates Vehicle.LastInspectionDate AND
    /// creates a MaintenanceLog entry — both committed atomically.
    /// </summary>
    public async Task<MaintenanceLogResponse> RecordInspection(
        Guid vehicleId, CreateLogRequest request, CancellationToken ct = default)
    {
        var vehicle = await _vehicleRepo.GetById(vehicleId, ct)
            ?? throw new KeyNotFoundException(VehicleDomainException.NotFound);

        vehicle.RecordInspection();

        var log = MaintenanceLog.Create(
            vehicleId:   vehicleId,
            description: request.Description,
            technician:  request.TechnicianName,
            cost:        request.Cost);

        await _maintenanceRepo.Add(log, ct);
        await _unitOfWork.SaveChanges(ct);
        return log.ToResponse();
    }

    public async Task<MaintenanceLogResponse> AddMaintenanceLog(
        CreateLogRequest request, CancellationToken ct = default)
    {
        await _vehicleRepo.GetById(request.VehicleId, ct)
            ?? throw new KeyNotFoundException(VehicleDomainException.NotFound);

        var log = MaintenanceLog.Create(
            vehicleId:   request.VehicleId,
            description: request.Description,
            technician:  request.TechnicianName,
            cost:        request.Cost);

        await _maintenanceRepo.Add(log, ct);
        await _unitOfWork.SaveChanges(ct);
        return log.ToResponse();
    }

    public async Task<IEnumerable<MaintenanceLogResponse>> GetMaintenanceHistory(
        Guid vehicleId, CancellationToken ct = default)
        => (await _maintenanceRepo.GetByVehicleId(vehicleId, ct)).Select(log => log.ToResponse());

    /// <summary>
    /// Generates a cryptographically random API key, stores only its BCrypt hash,
    /// returns the plain-text key once. The caller must securely store it —
    /// it cannot be recovered after this call.
    /// If an active key already exists it is revoked first.
    /// </summary>
    public async Task<ProvisionApiKeyResponse> ProvisionApiKey(
        Guid vehicleId, CancellationToken ct = default)
    {
        var vehicle = await _vehicleRepo.GetById(vehicleId, ct)
            ?? throw new KeyNotFoundException(VehicleDomainException.NotFound);
 
        var rawKey  = Convert.ToBase64String(
                          System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        var keyHash = BCrypt.Net.BCrypt.HashPassword(rawKey);
 
        vehicle.SetApiKey(keyHash); // domain method validates the BCrypt format
        await _unitOfWork.SaveChanges(ct);
 
        return new ProvisionApiKeyResponse(
            VehicleId:    vehicleId,
            PlainTextKey: rawKey,   // returned ONCE — never stored
            Label:        "Provisioned",
            CreatedAt:    DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Revokes the vehicle's current API key by clearing the stored hash.
    /// The vehicle simulator will be unable to authenticate until a new key
    /// is provisioned. Use when a key is compromised or the vehicle is
    /// decommissioned.
    /// </summary>
    public async Task RevokeApiKey(Guid vehicleId, CancellationToken ct = default)
    {
        var vehicle = await _vehicleRepo.GetById(vehicleId, ct)
            ?? throw new KeyNotFoundException(VehicleDomainException.NotFound);

        if (string.IsNullOrEmpty(vehicle.ApiKeyHash))
            throw new VehicleDomainException(VehicleDomainException.NoActiveApiKey);

        vehicle.RevokeApiKey();
        await _unitOfWork.SaveChanges(ct);
    }
}
