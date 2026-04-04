namespace NovaDrive.Application.Mappings;

public static class SensorDiagnosticMappings
{
    public static SensorDiagnosticResponse ToResponse(this SensorDiagnostic d) => new(
        Id:            d.Id,
        VehicleId:     d.VehicleId,
        SensorType:    d.SensorType,
        ErrorCode:     d.ErrorCode,
        Severity:      d.Severity,
        RawSensorData: d.RawSensorData,
        Timestamp:     d.Timestamp);
}