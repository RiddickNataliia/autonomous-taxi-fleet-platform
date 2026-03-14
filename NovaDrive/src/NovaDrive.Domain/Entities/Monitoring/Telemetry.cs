namespace NovaDrive.Domain.Entities;

public class Telemetry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? RideId { get; set; } //enable to check telemetry by ride in case client complains
    public required Guid VehicleId { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    
    public required GpsLocation GpsCoordinates { get; init; }
    
    public double Speed { get; init; }
    public int BatteryPercentage { get; init; }
    public double InternalTemperature { get; init; }
}
