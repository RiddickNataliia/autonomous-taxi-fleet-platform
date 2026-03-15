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

    public override string ToString() => $"{Percentage}%";
}
