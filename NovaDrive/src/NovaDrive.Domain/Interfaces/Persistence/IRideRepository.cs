namespace NovaDrive.Domain.Interfaces;

public interface IRideRepository : IRepository<Ride>
{
    Task<IEnumerable<Ride>> GetByPassengerId(Guid passengerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ride>> GetByVehicleId(Guid vehicleId, CancellationToken cancellationToken = default);
    Task<Ride?> GetActiveByPassengerId(Guid passengerId, CancellationToken cancellationToken = default);
    Task<Ride?> GetPendingByVehicleId(Guid vehicleId, CancellationToken cancellationToken = default);
}