namespace NovaDrive.Domain.Entities;

public class SensorDiagnostic
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid VehicleId { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public SensorType SensorType { get; init; } = SensorType.Unknown;
    public required string ErrorCode { get; init; }
    public DiagnosticSeverity Severity { get; init; }

    public string? RawSensorData { get; init; }

    internal SensorDiagnostic() { }


    public static SensorDiagnostic Create(Guid vehicleId, SensorType sensorType, 
    string errorCode, DiagnosticSeverity severity, string? rawSensorData)
    {
        return new SensorDiagnostic
        {
            VehicleId     = vehicleId,
            SensorType    = sensorType,
            ErrorCode     = errorCode,
            Severity      = severity,
            RawSensorData = rawSensorData
        };
    }
}

