namespace NovaDrive.Tests.UnitTests;

public class ApiKeyTests
{
    private const string ValidHash = "$2a$12$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy";

    [Fact]
    public void Create_WithValidParameters_ReturnsApiKey()
    {
        var vehicleId = Guid.NewGuid();
        var label     = "Test Key";
        var before    = DateTimeOffset.UtcNow;

        var apiKey = VehicleApiKey.Create(vehicleId, ValidHash, label);

        Assert.Equal(vehicleId, apiKey.VehicleId);
        Assert.Equal(ValidHash, apiKey.KeyHash);
        Assert.Equal(label,     apiKey.Label);
        Assert.True(apiKey.CreatedAt >= before);
        Assert.True(apiKey.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Create_WithValidParameters_IsActiveByDefault()
    {
        var apiKey = VehicleApiKey.Create(Guid.NewGuid(), ValidHash);

        Assert.True(apiKey.IsActive);
        Assert.Null(apiKey.RevokedAt);
    }

    [Fact]
    public void Create_WithEmptyVehicleId_Throws()
    {
        var ex = Assert.Throws<APIKeyDomainException>(() =>
            VehicleApiKey.Create(Guid.Empty, ValidHash));
        Assert.Equal(APIKeyDomainException.InvalidApiKey, ex.Message);
    }

    [Fact]
    public void Create_WithEmptyKeyHash_Throws()
    {
        var ex = Assert.Throws<APIKeyDomainException>(() =>
            VehicleApiKey.Create(Guid.NewGuid(), ""));
        Assert.Equal(APIKeyDomainException.InvalidApiKey, ex.Message);
    }

    [Fact]
    public void Create_WithWhitespaceKeyHash_Throws()
    {
        var ex = Assert.Throws<APIKeyDomainException>(() =>
            VehicleApiKey.Create(Guid.NewGuid(), "   "));
        Assert.Equal(APIKeyDomainException.InvalidApiKey, ex.Message);
    }

    [Fact]
    public void Create_WithNonBcryptHash_Throws()
    {
        var ex = Assert.Throws<APIKeyDomainException>(() =>
            VehicleApiKey.Create(Guid.NewGuid(), "notabcrypthash"));
        Assert.Equal(APIKeyDomainException.InvalidApiKey, ex.Message);
    }

    [Fact]
    public void Create_WithCorrectPrefixButWrongLength_Throws()
    {
        var ex = Assert.Throws<APIKeyDomainException>(() =>
            VehicleApiKey.Create(Guid.NewGuid(), "$2a$12$tooshort"));
        Assert.Equal(APIKeyDomainException.InvalidApiKey, ex.Message);
    }

    [Fact]
    public void Revoke_WhenActive_SetsIsActiveFalseAndSetsRevokedAt()
    {
        var apiKey = VehicleApiKey.Create(Guid.NewGuid(), ValidHash, "Test Key");
        var before  = DateTimeOffset.UtcNow;

        apiKey.Revoke();

        Assert.False(apiKey.IsActive);
        Assert.NotNull(apiKey.RevokedAt);
        Assert.True(apiKey.RevokedAt >= before);
        Assert.True(apiKey.RevokedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_IsIdempotent()
    {
        var apiKey = VehicleApiKey.Create(Guid.NewGuid(), ValidHash, "Test Key");
        apiKey.Revoke();
        var revokedAt = apiKey.RevokedAt;

        apiKey.Revoke(); // second call

        Assert.False(apiKey.IsActive);
        Assert.Equal(revokedAt, apiKey.RevokedAt); // timestamp must not change
    }
}