namespace NovaDrive.Domain.Entities;

public class SensorDiagnostic
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid VehicleId { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    
    public required string SensorType { get; init; } // e.g., "Lidar", "Radar"
    public required string ErrorCode { get; init; }
    public DiagnosticSeverity Severity { get; init; }

    public string? RawSensorData { get; init; }
}