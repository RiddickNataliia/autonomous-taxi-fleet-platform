namespace NovaDrive.Domain.ValueObjects;
public record BatteryLevel
{
    public int Percentage { get; init; }
    public bool IsLow => Percentage < 15;
    public bool IsCritical => Percentage < 5;
    public bool CanBeActivated => Percentage >= 10;

    public BatteryLevel(int percentage)
    {
        Percentage = Math.Clamp(percentage, 0, 100);
    }
}