namespace NovaDrive.Infrastructure.Repositories;

internal sealed class RideRepository : BaseRepository<Ride>, IRideRepository
{
    public RideRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Returns all rides for a given passenger, ordered most-recent first.
    /// Used on the passenger dashboard to show their ride history.
    /// </summary>
    public async Task<IEnumerable<Ride>> GetByPassengerId(
        Guid passengerId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Where(r => r.PassengerId == passengerId)
            .OrderByDescending(r => r.RequestTime)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Returns all rides assigned to a given vehicle, ordered most-recent first.
    /// Used on the admin dashboard to show a vehicle's recent activity.
    /// </summary>
    public async Task<IEnumerable<Ride>> GetByVehicleId(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.RequestTime)
            .ToListAsync(cancellationToken);
}
