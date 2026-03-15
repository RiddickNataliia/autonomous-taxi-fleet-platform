namespace NovaDrive.Domain.Interfaces;

public interface ITelemetryRepository
{
    Task Add(Telemetry telemetry, CancellationToken cancellationToken = default);
    Task<IEnumerable<Telemetry>> GetByVehicleId(Guid vehicleId, int limit = 100, CancellationToken cancellationToken = default);
    Task<IEnumerable<Telemetry>> GetByRideId(Guid rideId, CancellationToken cancellationToken = default);
}