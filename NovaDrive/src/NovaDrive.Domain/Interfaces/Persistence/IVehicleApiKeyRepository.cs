namespace NovaDrive.Domain.Interfaces;

public interface IVehicleApiKeyRepository : IRepository<VehicleApiKey>
{

    Task<VehicleApiKey?> GetActiveByVehicleId(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<VehicleApiKey>> GetAllByVehicleId(
        Guid vehicleId,
        CancellationToken cancellationToken = default);
}