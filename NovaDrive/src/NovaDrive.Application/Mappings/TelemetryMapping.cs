namespace NovaDrive.Application.Mappings;

public static class TelemetryMapping
{
public static TelemetryResponse ToResponse(this Telemetry t) => new(
    VehicleId:           t.VehicleId,
    Latitude:            t.GpsCoordinates.Latitude,
    Longitude:           t.GpsCoordinates.Longitude,
    SpeedKmh:            t.Speed,
    BatteryPercentage:   t.BatteryPercentage,
    HardwareTemperature: t.InternalTemperature,
    Timestamp:           t.Timestamp);
}