namespace NovaDrive.Domain.ValueObjects;

public record Vin
{
    public string Value { get; }

    public Vin(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new VehicleDomainException(VehicleDomainException.InvalidVin);

        // VINs are exactly 17 chars, no I/O/Q
        if (value.Length != 17 || value.Any(c => c is 'I' or 'O' or 'Q'))
            throw new VehicleDomainException(VehicleDomainException.InvalidVin);

        Value = value.ToUpperInvariant();
    }

    public override string ToString() => Value;
}