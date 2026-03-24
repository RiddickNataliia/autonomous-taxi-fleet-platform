namespace NovaDrive.Tests.UnitTests;

public class GpsLocationTests
{
    //helpers:
    private static GpsLocation CreateLocation(
        double latitude = 50.8503,
        double longitude = 4.3517) => new(latitude, longitude);

    [Fact]
    public void Constructor_AcceptsValidCoordinates()
    {
        var location = CreateLocation();
        Assert.Equal(50.8503, location.Latitude);
        Assert.Equal(4.3517, location.Longitude);
    }

    [Theory]
    [InlineData(90.1)]
    [InlineData(-90.1)]
    public void Constructor_ThrowsIfLatitudeOutOfRange(double invalidLatitude)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GpsLocation(invalidLatitude, 0));
        Assert.Contains("Latitude must be between -90 and 90", ex.Message);
    }

    [Theory]
    [InlineData(180.1)]
    [InlineData(-180.1)]
    public void Constructor_ThrowsIfLongitudeOutOfRange(double invalidLongitude)
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new GpsLocation(0, invalidLongitude));
        Assert.Contains("Longitude must be between -180 and 180", ex.Message);
    }

    [Theory]
    [InlineData(90.0, 0)]
    [InlineData(-90.0, 0)]
    [InlineData(0, 180.0)]
    [InlineData(0, -180.0)]
    public void Constructor_AcceptsExactBoundaryCoordinates(double lat, double lon)
    {
        var location = new GpsLocation(lat, lon);
        Assert.Equal(lat, location.Latitude);
        Assert.Equal(lon, location.Longitude);
    }

    [Fact]
    public void DistanceTo_ReturnsZeroForSamePoint()
    {
        var location = CreateLocation();
        Assert.Equal(0, location.DistanceTo(location));
    }

    [Fact]
    public void DistanceTo_ReturnsCorrectDistance()
    {
        var brussels = new GpsLocation(50.8503, 4.3517);
        var ghent = new GpsLocation(51.0543, 3.7174);

        var distance = brussels.DistanceTo(ghent);
        Assert.InRange(distance, 49, 50);
    }

    [Fact]
    public void DistanceTo_IsSymmetric()
    {
        var brussels = new GpsLocation(50.8503, 4.3517);
        var ghent    = new GpsLocation(51.0543, 3.7174);

        Assert.Equal(brussels.DistanceTo(ghent), ghent.DistanceTo(brussels));
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var location = CreateLocation();
        Assert.Equal("(50.8503, 4.3517)", location.ToString());
    }

}