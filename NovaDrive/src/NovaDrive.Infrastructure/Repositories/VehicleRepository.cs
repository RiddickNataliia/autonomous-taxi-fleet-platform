namespace NovaDrive.Infrastructure.Repositories;

internal sealed class VehicleRepository : BaseRepository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Returns all vehicles whose status is active.
    /// Called by RideMatchingService to find candidates for a new ride.
    /// Returned as a list so the in-memory RideMatchingService can apply
    /// its Haversine/battery checks without extra round-trips.
    /// </summary>
    public async Task<IEnumerable<Vehicle>> GetAllActive(CancellationToken cancellationToken = default)
        => await DbSet
            .Where(v => v.Status == VehicleStatus.Active)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Looks up a vehicle by its VIN.
    /// VINs are unique, so this should return either one vehicle or null.
    /// Used during vehicle registration to prevent duplicate VINs, and in the admin dashboard to look up a vehicle by its VIN.
    /// </summary>

    public async Task<Vehicle?> GetByVin(string vin, CancellationToken cancellationToken = default)
    => await DbSet
        .FirstOrDefaultAsync(v => v.VIN.Value == vin.ToUpperInvariant(), cancellationToken);

    public async Task UpdateVitals(Guid vehicleId, GpsLocation location, BatteryLevel battery, CancellationToken cancellationToken = default)
    {
        var vehicle = await DbSet
            .FirstOrDefaultAsync(v => v.Id == vehicleId, cancellationToken);

        if (vehicle is null) return;

        vehicle.UpdateVitals(location, battery);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
