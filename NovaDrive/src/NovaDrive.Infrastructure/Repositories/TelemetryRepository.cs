namespace NovaDrive.Infrastructure.Repositories;

public class TelemetryRepository : ITelemetryRepository
{
    private readonly IMongoCollection<Telemetry> _collection;

    public TelemetryRepository(IMongoDatabase db)
    {
        _collection = db.GetCollection<Telemetry>("telemetry");
    }

    public async Task Add(Telemetry telemetry, CancellationToken cancellationToken = default)
        => await _collection.InsertOneAsync(telemetry, cancellationToken: cancellationToken);

    public async Task<IEnumerable<Telemetry>> GetByVehicleId(Guid vehicleId, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(t => t.VehicleId == vehicleId)
            .SortByDescending(t => t.Timestamp)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Telemetry>> GetByRideId(Guid rideId, CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(t => t.RideId == rideId)
            .SortByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }
}

