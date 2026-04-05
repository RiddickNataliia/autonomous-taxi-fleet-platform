namespace NovaDrive.Tests.UnitTests;

public class SensorDiagnosticTests
{
    [Fact]
    public void SensorDiagnostic_Create_SetsAllFields()
    {
        var vehicleId = Guid.NewGuid();
        var diagnostic = new SensorDiagnostic
        {
            VehicleId     = vehicleId,
            SensorType    = SensorType.Lidar,
            ErrorCode     = "ERR_001",
            Severity      = DiagnosticSeverity.Critical,
            RawSensorData = "{\"angle\": 45}"
        };

        Assert.Equal(vehicleId, diagnostic.VehicleId);
        Assert.Equal(SensorType.Lidar, diagnostic.SensorType);
        Assert.Equal("ERR_001", diagnostic.ErrorCode);
        Assert.Equal(DiagnosticSeverity.Critical, diagnostic.Severity);
        Assert.Equal("{\"angle\": 45}", diagnostic.RawSensorData);
        Assert.True(diagnostic.Timestamp <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void SensorDiagnostic_RawSensorData_CanBeNull()
    {
        var diagnostic = new SensorDiagnostic
        {
            VehicleId = Guid.NewGuid(),
            ErrorCode = "ERR_002",
        };
        Assert.Null(diagnostic.RawSensorData);
    }
}