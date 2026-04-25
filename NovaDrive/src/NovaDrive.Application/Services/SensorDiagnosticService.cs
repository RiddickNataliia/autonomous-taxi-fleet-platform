namespace NovaDrive.Application.Services;

public interface ISensorDiagnosticService
{
    Task LogDiagnostic(Guid vehicleId, LogDiagnosticRequest request);
    Task<IEnumerable<SensorDiagnosticResponse>> GetByVehicle(Guid vehicleId);
    Task<IEnumerable<SensorDiagnosticResponse>> GetBySeverity(DiagnosticSeverity severity);
}

public class SensorDiagnosticService : ISensorDiagnosticService
{
    private readonly ISensorDiagnosticRepository _repository;
    private readonly IVehicleRepository _vehicleRepository;

    public SensorDiagnosticService(
        ISensorDiagnosticRepository repository,
        IVehicleRepository vehicleRepository)
    {
        _repository        = repository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task LogDiagnostic(Guid vehicleId, LogDiagnosticRequest request)
    {
        var vehicle = await _vehicleRepository.GetById(vehicleId)
            ?? throw new KeyNotFoundException(VehicleDomainException.NotFound);

        var diagnostic = SensorDiagnostic.Create(
            vehicle.Id, request.SensorType, request.ErrorCode, 
            request.Severity, request.RawSensorData);

                await _repository.Add(diagnostic);
    }

    public async Task<IEnumerable<SensorDiagnosticResponse>> GetByVehicle(Guid vehicleId)
        => (await _repository.GetByVehicleId(vehicleId)).Select(d => d.ToResponse());

    public async Task<IEnumerable<SensorDiagnosticResponse>> GetBySeverity(DiagnosticSeverity severity)
        => (await _repository.GetBySeverity(severity)).Select(d => d.ToResponse());
}
