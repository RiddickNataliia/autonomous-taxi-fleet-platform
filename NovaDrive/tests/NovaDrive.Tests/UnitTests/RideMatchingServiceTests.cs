namespace NovaDrive.Tests.UnitTests;

public class RideMatchingServiceTests
{
    private readonly RideMatchingService _service = new();

    private static readonly GpsLocation PassengerLocation = new(50.8503, 4.3517); // Brussels

    // helpers
    private static Vehicle CreateActiveVehicle(
        double latitude,
        double longitude,
        int batteryPercentage = 100) => new(
        licensePlate:    "1-ABC-001",
        modelName:       "Tesla Model Y",
        vin:             new Vin("1HGCM82633A123456"),
        status:          VehicleStatus.Active,
        currentLocation: new GpsLocation(latitude, longitude),
        battery:         new BatteryLevel(batteryPercentage));

    private static Vehicle CreateInactiveVehicle(
        double latitude,
        double longitude) => new(
            licensePlate:    "1-ABC-002",
            modelName:       "Tesla Model Y",
            vin:             new Vin("1HGCM82633A123457"),
            status:          VehicleStatus.Inactive,
            currentLocation: new GpsLocation(latitude, longitude));

    // --- no vehicles ---

    [Fact]
    public void FindBestMatch_EmptyFleet_ReturnsNull()
    {
        var result = _service.FindBestMatch(PassengerLocation, [], estimatedDistanceKm: 10.0);
        Assert.Null(result);
    }

    [Fact]
    public void FindBestMatch_NoActiveVehicles_ReturnsNull()
    {
        var vehicles = new[]
        {
            CreateInactiveVehicle(50.8503, 4.3517),
            CreateInactiveVehicle(50.8600, 4.3600)
        };

        var result = _service.FindBestMatch(PassengerLocation, vehicles, estimatedDistanceKm: 10.0);
        Assert.Null(result);
    }

    [Fact]
    public void FindBestMatch_AllVehiclesBeyondMaxDistance_ReturnsNull()
    {
        // Antwerp is ~45km from Brussels — beyond the 40km max
        var vehicles = new[] { CreateActiveVehicle(51.2194, 4.4025) };

        var result = _service.FindBestMatch(PassengerLocation, vehicles, estimatedDistanceKm: 10.0);
        Assert.Null(result);
    }

    [Fact]
    public void FindBestMatch_InsufficientBatteryForTotalDistance_ReturnsNull()
    {
        // vehicle is ~1km from passenger, ride is 30km
        // total = ~31km, needs (31/3) * 1.10 ≈ 11.4% → 12% minimum
        // vehicle has 11% — not enough
        var vehicle = CreateActiveVehicle(50.8570, 4.3550, batteryPercentage: 11);

        var result = _service.FindBestMatch(PassengerLocation, [vehicle], estimatedDistanceKm: 30.0);
        Assert.Null(result);
    }


    // --- happy path ---

    [Fact]
    public void FindBestMatch_SingleActiveVehicle_ReturnsIt()
    {
        var vehicles = new[] { CreateActiveVehicle(50.8600, 4.3600) };

        var result = _service.FindBestMatch(PassengerLocation, vehicles, estimatedDistanceKm: 10.0);
        Assert.NotNull(result);
    }

    [Fact]
    public void FindBestMatch_ReturnsNearestVehicle()
    {
        // near: ~1km from passenger, far: ~5km from passenger
        var near = CreateActiveVehicle(50.8570, 4.3550);
        var far  = CreateActiveVehicle(50.8900, 4.4000);

        var result = _service.FindBestMatch(PassengerLocation, [near, far], estimatedDistanceKm: 10.0);
        Assert.Equal(near.Id, result!.Id);
    }

    [Fact]
    public void FindBestMatch_ReturnsNearestVehicle_RegardlessOfInputOrder()
    {
        var near = CreateActiveVehicle(50.8570, 4.3550);
        var far  = CreateActiveVehicle(50.8900, 4.4000);

        // far listed first
        var result = _service.FindBestMatch(PassengerLocation, [far, near], estimatedDistanceKm: 10.0);
        Assert.Equal(near.Id, result!.Id);
    }

    // --- filters combined ---

    [Fact]
    public void FindBestMatch_SkipsInactiveVehicles_ReturnsNearestActive()
    {
        // inactive vehicle is closer but should be skipped
        var inactive = CreateInactiveVehicle(50.8520, 4.3530);
        var active   = CreateActiveVehicle(50.8700, 4.3700);

        var result = _service.FindBestMatch(PassengerLocation, [inactive, active], estimatedDistanceKm: 10.0);
        Assert.Equal(active.Id, result!.Id);
    }

    [Fact]
    public void FindBestMatch_SkipsVehiclesWithInsufficientBattery_ReturnsNextNearest()
    {
        // near vehicle has low battery for the distance, far vehicle has enough
        // 30km needs (30/3) * 1.10 = 11% — nearLowBattery has 10%, farGoodBattery has 50%
        var nearLowBattery  = CreateActiveVehicle(50.8570, 4.3550, batteryPercentage: 10);
        var farGoodBattery  = CreateActiveVehicle(50.8700, 4.3700, batteryPercentage: 50);

        var result = _service.FindBestMatch(PassengerLocation, [nearLowBattery, farGoodBattery], estimatedDistanceKm: 30.0);
        Assert.Equal(farGoodBattery.Id, result!.Id);
    }

    [Fact]
    public void FindBestMatch_VehicleWithinMaxDistance_IsIncluded()
    {
        // within 40km limit
        var vehicle = CreateActiveVehicle(50.8700, 4.3600);

        var result = _service.FindBestMatch(PassengerLocation, [vehicle], estimatedDistanceKm: 10.0);
        Assert.NotNull(result);
    }

    [Fact]
    public void FindBestMatch_VehicleBeyondMaxDistance_IsExcluded()
    {
        // beyond the 40km limit
        var vehicle = CreateActiveVehicle(51.2194, 4.4025);

        var result = _service.FindBestMatch(PassengerLocation, [vehicle], estimatedDistanceKm: 10.0);
        Assert.Null(result);
    }
}