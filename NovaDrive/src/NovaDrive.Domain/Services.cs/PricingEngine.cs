namespace NovaDrive.Domain.Services;

public class PricingEngine
{
    private const decimal VatRate = 0.21m; // 21% VAT
    private const decimal MinFare = 5.00m; // Minimum 5 Euro rule

    public PricingResult CalculateFinalPrice(decimal baseFare, int availablePoints, DiscountCode? code)
    {
        // Start with the raw distance/time amount
        decimal currentFare = baseFare;

        // 1. Loyalty Discount: €1 per 100 points
        // Restriction: May never exceed 20% of the current fare.
        decimal potentialLoyaltyDiscount = (availablePoints / 100) * 1.00m;
        decimal loyaltyCap = currentFare * 0.20m;
        
        decimal actualLoyaltyDiscount = Math.Min(potentialLoyaltyDiscount, loyaltyCap);
        
        // Calculate points actually "spent" based on the capped discount
        int pointsSpent = (int)(actualLoyaltyDiscount * 100);
        
        currentFare -= actualLoyaltyDiscount;

        // 2. Discount Codes: Applied to the REMAINING amount
        decimal codeDiscount = 0;
        if (code != null && code.IsValid(currentFare))
        {
            codeDiscount = code.CalculateDiscount(currentFare);
            currentFare -= codeDiscount;
        }

        // 3. Minimum Fare & VAT
        // We ensure the price doesn't drop below the minimum before adding tax
        decimal finalNet = Math.Max(MinFare, Math.Round(currentFare, 2));
        
        decimal vatAmount = Math.Round(finalNet * VatRate, 2);
        decimal totalGross = finalNet + vatAmount;

        return new PricingResult(
            NetAmount: finalNet,
            VatAmount: vatAmount,
            TotalGross: totalGross,
            LoyaltyDiscountApplied: actualLoyaltyDiscount,
            PointsUsed: pointsSpent
        );
    }
}