namespace NovaDrive.Domain.Interfaces;

public interface IMaintenanceLogRepository : IRepository<MaintenanceLog>
{
    Task<IEnumerable<MaintenanceLog>> GetByVehicleId(Guid vehicleId, CancellationToken cancellationToken = default);
}