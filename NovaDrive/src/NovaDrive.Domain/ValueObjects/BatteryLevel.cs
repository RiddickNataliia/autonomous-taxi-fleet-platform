namespace NovaDrive.Domain.ValueObjects;
public record BatteryLevel
{
    public int Percentage { get; }
    public bool IsLow => Percentage < 15;
    public bool IsCritical => Percentage < 5;

    public BatteryLevel(int percentage)
    {
        Percentage = Math.Clamp(percentage, 0, 100);
    }

    public bool IsSufficientFor(double distanceKm)
    {
        // Rough estimate: 1% battery ≈ 3km range, with 10% safety buffer
        const double KmPerPercent = 3.0;
        const double SafetyBuffer = 1.10;

        return Percentage >= (distanceKm / KmPerPercent) * SafetyBuffer;
    }

    public override string ToString() => $"{Percentage}%";
}
