namespace NovaDrive.Infrastructure.Repositories;

internal sealed class VehicleApiKeyRepository
    : BaseRepository<VehicleApiKey>, IVehicleApiKeyRepository
{
    public VehicleApiKeyRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Returns the single active key for a vehicle.
    /// </summary>
    public async Task<VehicleApiKey?> GetActiveByVehicleId(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Where(k => k.VehicleId == vehicleId && k.IsActive)
            .OrderByDescending(k => k.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Full audit history — all keys for the vehicle, newest first.
    /// </summary>
    public async Task<IEnumerable<VehicleApiKey>> GetAllByVehicleId(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Where(k => k.VehicleId == vehicleId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(cancellationToken);
}