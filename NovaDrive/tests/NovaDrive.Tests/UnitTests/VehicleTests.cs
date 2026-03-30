namespace NovaDrive.Tests.UnitTests;

public class VehicleTests
{
    //helpers:

    private static Vehicle CreateVehicle() =>
        new("AB-123-CD", "Tesla Model Y", new Vin("1HGBH41JXMN109186"));

    private static Vehicle CreateInspectedVehicle()
    {
        var vehicle = CreateVehicle();
        vehicle.RecordInspection();
        return vehicle;
    }

    private static Vehicle CreateActiveVehicle()
    {
        var vehicle = CreateInspectedVehicle();
        vehicle.Activate();
        return vehicle;
    }

    private static Vehicle CreateEnRouteVehicle()
    {
        var vehicle = CreateActiveVehicle();

        typeof(Vehicle)
            .GetProperty(nameof(Vehicle.Status))!
            .SetValue(vehicle, VehicleStatus.EnRoute);
        return vehicle;
    }

    private const string ValidBcryptHash = "$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy";


    [Fact]
    public void Activate_WhenInspectedAndGoodBattery_SetsStatusToActive()
    {
        // Arrange
        var vehicle = CreateInspectedVehicle();
        // Act
        vehicle.Activate();
        // Assert
        Assert.Equal(VehicleStatus.Active, vehicle.Status);
    }

    [Fact]
    public void Activate_WhenNeverInspected_Throws()
    {
        var vehicle = CreateVehicle(); // no inspection recorded

        var ex = Assert.Throws<VehicleDomainException>(() => vehicle.Activate());
        Assert.Equal(VehicleDomainException.InspectionRequired, ex.Message);
    }

    [Fact]
    public void Activate_WhenInspectionOverdue_Throws()
    {
        var vehicle = CreateVehicle();

        // Set last inspection to over 12 months ago via reflection
        typeof(Vehicle)
            .GetProperty(nameof(Vehicle.LastInspectionDate))!
            .SetValue(vehicle, DateTimeOffset.UtcNow.AddYears(-1).AddDays(-1));

        var ex = Assert.Throws<VehicleDomainException>(() => vehicle.Activate());
        Assert.Equal(VehicleDomainException.InspectionRequired, ex.Message);
    }

    [Fact]
    public void Activate_WhenBatteryIsLow_Throws()
    {
        var vehicle = CreateInspectedVehicle();

        // Update battery to low level
        vehicle.UpdateVitals(new GpsLocation(0, 0), new BatteryLevel(10));

        var ex = Assert.Throws<VehicleDomainException>(() => vehicle.Activate());
        Assert.Equal(VehicleDomainException.LowBattery, ex.Message);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_IsIdempotent()
    {
        var vehicle = CreateActiveVehicle();

        vehicle.Activate(); // second call

        Assert.Equal(VehicleStatus.Active, vehicle.Status);
    }


    [Fact]
    public void Deactivate_WhenActive_SetsStatusToInactive()
    {
        var vehicle = CreateActiveVehicle();

        vehicle.Deactivate();

        Assert.Equal(VehicleStatus.Inactive, vehicle.Status);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_IsIdempotent()
    {
        var vehicle = CreateVehicle(); // starts Inactive

        vehicle.Deactivate(); // second call

        Assert.Equal(VehicleStatus.Inactive, vehicle.Status);
    }


    [Fact]
    public void MarkForMaintenance_SetsStatusToMaintenance()
    {
        var vehicle = CreateActiveVehicle();

        vehicle.MarkForMaintenance();

        Assert.Equal(VehicleStatus.Maintenance, vehicle.Status);
    }

    [Fact]
    public void MarkForMaintenance_WhenAlreadyMaintenance_IsIdempotent()
    {
        var vehicle = CreateActiveVehicle();
        vehicle.MarkForMaintenance();

        vehicle.MarkForMaintenance(); // second call

        Assert.Equal(VehicleStatus.Maintenance, vehicle.Status);
    }


    [Fact]
    public void RecordInspection_SetsLastInspectionDate()
    {
        var vehicle = CreateVehicle();
        var before  = DateTimeOffset.UtcNow;

        vehicle.RecordInspection();

        Assert.NotNull(vehicle.LastInspectionDate);
        Assert.True(vehicle.LastInspectionDate >= before);
        Assert.True(vehicle.LastInspectionDate <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void RecordInspection_AllowsVehicleToBeActivated()
    {
        var vehicle = CreateVehicle(); // never inspected

        vehicle.RecordInspection();
        vehicle.Activate(); // should not throw

        Assert.Equal(VehicleStatus.Active, vehicle.Status);
    }


    [Fact]
    public void NeedsInspection_WhenNeverInspected_ReturnsTrue()
    {
        var vehicle = CreateVehicle();

        Assert.True(vehicle.NeedsInspection());
    }

    [Fact]
    public void NeedsInspection_WhenInspectionOverdue_ReturnsTrue()
    {
        var vehicle = CreateVehicle();

        typeof(Vehicle)
            .GetProperty(nameof(Vehicle.LastInspectionDate))!
            .SetValue(vehicle, DateTimeOffset.UtcNow.AddYears(-1).AddDays(-1));

        Assert.True(vehicle.NeedsInspection());
    }

    [Fact]
    public void NeedsInspection_WhenRecentlyInspected_ReturnsFalse()
    {
        var vehicle = CreateVehicle();

        vehicle.RecordInspection();

        Assert.False(vehicle.NeedsInspection());
    }

    [Fact]
    public void NeedsInspection_WhenInspectedExactly12MonthsAgo_ReturnsTrue()
    {
        var vehicle = CreateVehicle();

        typeof(Vehicle)
            .GetProperty(nameof(Vehicle.LastInspectionDate))!
            .SetValue(vehicle, DateTimeOffset.UtcNow.AddYears(-1));

        // exactly 12 months ago — the check is strictly less than so this is overdue
        Assert.True(vehicle.NeedsInspection());
    }


    [Fact]
    public void UpdateVitals_SetsLocationAndBattery()
    {
        var vehicle     = CreateActiveVehicle();
        var newLocation = new GpsLocation(51.05, 3.72);
        var newBattery  = new BatteryLevel(75);

        vehicle.UpdateVitals(newLocation, newBattery);

        Assert.Equal(51.05, vehicle.CurrentLocation.Latitude);
        Assert.Equal(3.72,  vehicle.CurrentLocation.Longitude);
        Assert.Equal(75,    vehicle.Battery.Percentage);
    }

    [Fact]
    public void UpdateVitals_WhenEnRouteAndBatteryCritical_SetsStatusToMaintenance()
    {
        var vehicle = CreateEnRouteVehicle();

        vehicle.UpdateVitals(new GpsLocation(0, 0), new BatteryLevel(0));

        Assert.Equal(VehicleStatus.Maintenance, vehicle.Status);
    }

    [Fact]
    public void UpdateVitals_WhenActiveAndBatteryCritical_SetsStatusToInactive()
    {
        var vehicle = CreateActiveVehicle();

        vehicle.UpdateVitals(new GpsLocation(0, 0), new BatteryLevel(0));

        Assert.Equal(VehicleStatus.Inactive, vehicle.Status);
    }

    [Fact]
    public void UpdateVitals_WhenBatteryNotCritical_DoesNotChangeStatus()
    {
        var vehicle = CreateActiveVehicle();

        vehicle.UpdateVitals(new GpsLocation(0, 0), new BatteryLevel(20));

        Assert.Equal(VehicleStatus.Active, vehicle.Status);
    }


    // [Fact]
    // public void SetApiKey_WithValidHash_StoresKey()
    // {
    //     var vehicle = CreateInspectedVehicle();

    //     vehicle.SetApiKey(ValidBcryptHash);

    //     Assert.Equal(ValidBcryptHash, vehicle.ApiKeyHash);
    // }

    // [Fact]
    // public void SetApiKey_WithEmptyString_Throws()
    // {
    //     var vehicle = CreateInspectedVehicle();

    //     var ex = Assert.Throws<VehicleDomainException>(() => vehicle.SetApiKey(""));
    //     Assert.Equal(VehicleDomainException.InvalidApiKey, ex.Message);
    // }

    // [Fact]
    // public void SetApiKey_WithWhitespace_Throws()
    // {
    //     var vehicle = CreateInspectedVehicle();

    //     var ex = Assert.Throws<VehicleDomainException>(() => vehicle.SetApiKey("   "));
    //     Assert.Equal(VehicleDomainException.InvalidApiKey, ex.Message);
    // }

    // [Fact]
    // public void SetApiKey_WithNonBcryptFormat_Throws()
    // {
    //     var vehicle = CreateInspectedVehicle();

    //     var ex = Assert.Throws<VehicleDomainException>(() => vehicle.SetApiKey("notabcrypthash"));
    //     Assert.Equal(VehicleDomainException.InvalidApiKey, ex.Message);
    // }

    // [Fact]
    // public void SetApiKey_WithCorrectPrefixButWrongLength_Throws()
    // {
    //     var vehicle = CreateInspectedVehicle();

    //     var ex = Assert.Throws<VehicleDomainException>(() => vehicle.SetApiKey("$2a$12$tooshort"));
    //     Assert.Equal(VehicleDomainException.InvalidApiKey, ex.Message);
    // }

    // [Fact]
    // public void SetApiKey_WhenEnRoute_Throws()
    // {
    //     var vehicle = CreateEnRouteVehicle();

    //     var ex = Assert.Throws<VehicleDomainException>(() => vehicle.SetApiKey(ValidBcryptHash));
    //     Assert.Equal(VehicleDomainException.CannotRotateKeyWhileEnRoute, ex.Message);
    // }

    // [Fact]
    // public void SetApiKey_WithSameKey_IsIdempotent()
    // {
    //     var vehicle = CreateInspectedVehicle();
    //     vehicle.SetApiKey(ValidBcryptHash);

    //     vehicle.SetApiKey(ValidBcryptHash); // same key again

    //     Assert.Equal(ValidBcryptHash, vehicle.ApiKeyHash);
    // }
}