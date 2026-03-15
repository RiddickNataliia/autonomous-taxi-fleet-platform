namespace NovaDrive.Domain.ValueObjects;
public record Vin
{
    public string Value { get; init; }

    public Vin(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 17)
            throw new DomainException("Invalid VIN format. Must be 17 characters.");
        
        Value = value.ToUpper();
    }
}