namespace NovaDrive.Tests.UnitTests;

public class VinTests
{
    [Fact]
    public void Constructor_AcceptsValidVin()
    {
        var vin = new Vin("1HGCM82633A004352");
        Assert.Equal("1HGCM82633A004352", vin.Value);
    }

    [Fact]
    public void Constructor_NormalizesToUppercase()
    {
        var vin = new Vin("1hgcm82633a004352");
        Assert.Equal("1HGCM82633A004352", vin.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsIfNullOrWhitespace(string invalid)
    {
        Assert.Throws<VehicleDomainException>(() => new Vin(invalid));
    }

    [Fact]
    public void Constructor_ThrowsIfNot17Characters()
    {
        Assert.Throws<VehicleDomainException>(() => new Vin("1HGCM82633A00435")); // 16 chars
        Assert.Throws<VehicleDomainException>(() => new Vin("1HGCM82633A004352X")); // 18 chars
    }

    [Theory]
    [InlineData("1HGCM82633I004352")] // contains I
    [InlineData("1HGCM82633O004352")] // contains O
    [InlineData("1HGCM82633Q004352")] // contains Q
    public void Constructor_ThrowsIfContainsIllegalCharacters(string invalid)
    {
        Assert.Throws<VehicleDomainException>(() => new Vin(invalid));
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var vin = new Vin("1HGCM82633A004352");
        Assert.Equal("1HGCM82633A004352", vin.ToString());
    }
}