namespace NovaDrive.Infrastructure.Repositories;

public class SensorDiagnosticRepository : ISensorDiagnosticRepository
{
    private readonly IMongoCollection<SensorDiagnostic> _collection;
    public SensorDiagnosticRepository(IMongoDatabase db)
    {
        _collection = db.GetCollection<SensorDiagnostic>("sensor_diagnostics");
    }

    /// <summary>
    /// Persists a new sensor diagnostic entry to MongoDB.
    /// Called by the vehicle simulator whenever a Lidar, Radar, or Camera
    /// system reports a deviation.
    /// </summary>
    public async Task Add(SensorDiagnostic diagnostic,
        CancellationToken cancellationToken = default)
        => await _collection.InsertOneAsync(diagnostic,
            cancellationToken: cancellationToken);

    /// <summary>
    /// Returns all diagnostic records for a specific vehicle, ordered by
    /// most recent first. Used by the admin dashboard to inspect the fault
    /// history of a single robotaxi.
    /// </summary>
    public async Task<IEnumerable<SensorDiagnostic>> GetByVehicleId(
        Guid vehicleId,
        CancellationToken cancellationToken = default)
        => await _collection
            .Find(d => d.VehicleId == vehicleId)
            .SortByDescending(d => d.Timestamp)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Returns all diagnostic records across the entire fleet that match a
    /// given severity level, ordered by most recent first. Used by the admin
    /// dashboard to surface critical faults that require immediate attention.
    /// </summary>
    public async Task<IEnumerable<SensorDiagnostic>> GetBySeverity(
        DiagnosticSeverity severity,
        CancellationToken cancellationToken = default)
        => await _collection
            .Find(d => d.Severity == severity)
            .SortByDescending(d => d.Timestamp)
            .ToListAsync(cancellationToken);
}