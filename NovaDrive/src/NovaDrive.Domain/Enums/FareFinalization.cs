namespace NovaDrive.Domain.Services;

public class FareFinalizationService
{
    private const decimal MinimumFare = 5.00m;
    private const decimal VatRate = 0.21m; // Example 21% VAT

    public Money FinalizeFare(Money calculatedFare, User user, DiscountCode? promoCode)
    {
        decimal currentAmount = calculatedFare.Amount;

        // RULE 1: Loyalty Discount (100 points = €1)
        // Restriction: Max 20% of current fare
        decimal maxLoyaltyDiscount = currentAmount * 0.20m;
        decimal potentialLoyaltyDiscount = user.LoyaltyPoints / 100m;
        decimal actualLoyaltyDiscount = Math.Min(maxLoyaltyDiscount, potentialLoyaltyDiscount);
        
        currentAmount -= actualLoyaltyDiscount;

        // RULE 2: Discount Codes
        if (promoCode != null && promoCode.IsValid(currentAmount))
        {
            if (promoCode.Type == DiscountType.Percentage)
                currentAmount -= currentAmount * (promoCode.Value / 100m);
            else if (promoCode.Type == DiscountType.Flat)
                currentAmount -= promoCode.Value;
        }

        // RULE 3: Hard Business Rules (Min Fare & No Negatives)
        currentAmount = Math.Max(currentAmount, MinimumFare);

        // RULE 4: VAT Calculation
        decimal finalPrice = Math.Round(currentAmount, 2);

        return new Money(finalPrice, "EUR");
    }
}