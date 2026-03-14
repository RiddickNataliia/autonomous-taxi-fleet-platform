namespace NovaDrive.Domain.Interfaces;

public interface IPaymentGateway
{
    // Returns a transaction reference string if successful
    Task<string> ProcessPaymentAsync(decimal amount, Currency currency);
}