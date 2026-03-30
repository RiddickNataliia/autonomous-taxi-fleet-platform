namespace NovaDrive.Domain.Entities;

public class VehicleApiKey
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid VehicleId { get; init; }
    public required string KeyHash { get; init; }
    public string Label { get; init; } = string.Empty;//(e.g. "Initial provisioning", "Rotated after incident #42")
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; private set; }
    private VehicleApiKey() { }

    /// <summary>
    /// Factory — creates a new active API key entry.
    /// </summary>
    /// <param name="vehicleId">The vehicle being provisioned.</param>
    /// <param name="keyHash">BCrypt hash of the generated plain-text key.</param>
    /// <param name="label">Optional description for the admin UI.</param>
    public static VehicleApiKey Create(Guid vehicleId, string keyHash, string label = "")
    {
        if (vehicleId == Guid.Empty)
            throw new APIKeyDomainException(APIKeyDomainException.InvalidApiKey);

        if (string.IsNullOrWhiteSpace(keyHash))
            throw new APIKeyDomainException(APIKeyDomainException.InvalidApiKey);

        // Validate it really is a BCrypt hash (starts with $2a/$2b/$2y, length 60)
        if ((!keyHash.StartsWith("$2a") && !keyHash.StartsWith("$2b") && !keyHash.StartsWith("$2y"))
            || keyHash.Length != 60)
            throw new APIKeyDomainException(APIKeyDomainException.InvalidApiKey);

        return new VehicleApiKey
        {
            VehicleId = vehicleId,
            KeyHash   = keyHash,
            Label     = label
        };
    }

    /// <summary>Revokes this key — it can no longer be used for authentication.</summary>
    public void Revoke()
    {
        if (!IsActive) return;
        IsActive  = false;
        RevokedAt = DateTimeOffset.UtcNow;
    }
}
