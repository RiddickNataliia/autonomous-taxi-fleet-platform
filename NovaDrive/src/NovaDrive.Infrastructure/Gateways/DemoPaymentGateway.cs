namespace NovaDrive.Infrastructure.Payments;

public class DemoPaymentGateway : IPaymentGateway
{
    public async Task<string> ProcessPaymentAsync(decimal amount, Currency currency)
    {
        // 1. Simulate network lag
        await Task.Delay(1000); 

        // 2. Logic for the demo: 
        // If the amount is exactly 666, simulate a failure.
        if (amount == 666m)
        {
            throw new Exception("Payment Gateway: Card Declined (Demo Mode)");
        }

        // 3. Return a fake Bank Reference
        return $"DEMO_TX_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}