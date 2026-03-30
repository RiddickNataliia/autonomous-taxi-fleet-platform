namespace NovaDrive.Infrastructure.Repositories;

internal sealed class MaintenanceLogRepository : BaseRepository<MaintenanceLog>, IMaintenanceLogRepository
{
    public MaintenanceLogRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Returns the full maintenance history for a vehicle, newest first.
    /// The most recent log is used to determine the last service date and
    /// whether a follow-up inspection is due.
    /// </summary>
    public async Task<IEnumerable<MaintenanceLog>> GetByVehicleId(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Where(m => m.VehicleId == vehicleId)
            .OrderByDescending(m => m.ServiceDate)
            .ToListAsync(cancellationToken);
}
