namespace NovaDrive.Infrastructure.Repositories;

public class TelemetryRepository : ITelemetryRepository
{
    private readonly IMongoCollection<Telemetry> _collection;

    public TelemetryRepository(IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString("Mongo"));
        var db = client.GetDatabase("novadrive");
        _collection = db.GetCollection<Telemetry>("telemetry");
    }

    public async Task AddAsync(Telemetry telemetry)
        => await _collection.InsertOneAsync(telemetry);

    public async Task<IEnumerable<Telemetry>> GetByVehicleIdAsync(Guid vehicleId, int limit = 100)
    {
        return await _collection
            .Find(t => t.VehicleId == vehicleId)
            .SortByDescending(t => t.RecordedAt)
            .Limit(limit)
            .ToListAsync();
    }
}