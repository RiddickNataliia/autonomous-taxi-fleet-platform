namespace NovaDrive.Infrastructure.Payments;

/// <summary>
/// Demo (stub) payment gateway used in development and testing.
/// Simulates network latency and a predictable failure case (amount == 666).
/// </summary>
public class DemoPaymentGateway : IPaymentGateway
{
    public async Task<string> ProcessPayment(decimal amount, Currency currency)
    {
        // 1. Simulate network lag for round trip to external payment gateway
        await Task.Delay(300); 

        // 2. Logic for the demo: 
        // If the amount is exactly 666, simulate a failure (Transaction.MarkAsFailed)
        if (amount == 666m)
        {
            throw new Exception("Payment Gateway: Card Declined (Demo Mode - amount 666 is reserved for failure testing).");
        }

        // 3. Return a fake realistic looking Bank Reference
        return $"DEMO-{currency}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
    }
}