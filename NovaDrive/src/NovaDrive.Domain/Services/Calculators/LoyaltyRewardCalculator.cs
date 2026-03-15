namespace NovaDrive.Domain.Services;

public class LoyaltyRewardCalculator
{
    /// <summary>
    /// Calculates points earned. €1 spent = 10 points
    /// </summary>
    public int CalculateEarnedPoints(decimal totalGross, bool isPromoPeriod)
    {
        // Ensure we don't calculate points on negative numbers
        if (totalGross <= 0) return 0;

        int basePoints = (int)(totalGross * 10);
        return isPromoPeriod ? basePoints * 2 : basePoints;
    }
}