namespace NovaDrive.Tests.UnitTests;

public class BatteryLevelTests
{
    [Fact]
    public void Constructor_ClampsAbove100()
    {
        var battery = new BatteryLevel(150);
        Assert.Equal(100, battery.Percentage);
    }

    [Fact]
    public void Constructor_ClampsBelow0()
    {
        var battery = new BatteryLevel(-10);
        Assert.Equal(0, battery.Percentage);
    }

    [Fact]
    public void Constructor_AcceptsValidPercentage()
    {
        var battery = new BatteryLevel(75);
        Assert.Equal(75, battery.Percentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Constructor_AcceptsBoundaryValues(int percentage)
    {
        var battery = new BatteryLevel(percentage);
        Assert.Equal(percentage, battery.Percentage);
    }

    [Fact]
    public void IsLow_WhenBelow15_ReturnsTrue()
    {
        Assert.True(new BatteryLevel(14).IsLow);
    }

    [Fact]
    public void IsLow_WhenExactly15_ReturnsFalse()
    {
        Assert.False(new BatteryLevel(15).IsLow);
    }

    [Fact]
    public void IsLow_WhenAbove15_ReturnsFalse()
    {
        Assert.False(new BatteryLevel(50).IsLow);
    }

    [Fact]
    public void IsCritical_WhenBelow5_ReturnsTrue()
    {
        Assert.True(new BatteryLevel(4).IsCritical);
    }

    [Fact]
    public void IsCritical_WhenExactly5_ReturnsFalse()
    {
        Assert.False(new BatteryLevel(5).IsCritical);
    }

    [Fact]
    public void IsCritical_WhenAbove5_ReturnsFalse()
    {
        Assert.False(new BatteryLevel(50).IsCritical);
    }

    [Fact]
    public void IsSufficientFor_WhenBatteryEnough_ReturnsTrue()
    {
        // 30km needs (30/3) * 1.10 = 11% — 11% is exactly enough
        var battery = new BatteryLevel(11);
        Assert.True(battery.IsSufficientFor(30.0));
    }

    [Fact]
    public void IsSufficientFor_WhenBatteryNotEnough_ReturnsFalse()
    {
        // 30km needs 11% — 10% is not enough
        var battery = new BatteryLevel(10);
        Assert.False(battery.IsSufficientFor(30.0));
    }

    [Fact]
    public void IsSufficientFor_ZeroDistance_AlwaysReturnsTrue()
    {
        var battery = new BatteryLevel(0);
        Assert.True(battery.IsSufficientFor(0.0));
    }

    [Fact]
    public void IsSufficientFor_FullBattery_SufficientForLongDistance()
    {
        // 100% * 3km = 300km range — sufficient for 200km
        var battery = new BatteryLevel(100);
        Assert.True(battery.IsSufficientFor(200.0));
    }

    [Fact]
    public void ToString_ReturnsFormattedPercentage()
    {
        Assert.Equal("75%", new BatteryLevel(75).ToString());
    }

    [Fact]
    public void Equality_SamePercentage_AreEqual()
    {
        Assert.Equal(new BatteryLevel(50), new BatteryLevel(50));
    }

    [Fact]
    public void Equality_DifferentPercentage_AreNotEqual()
    {
        Assert.NotEqual(new BatteryLevel(50), new BatteryLevel(51));
    }
}