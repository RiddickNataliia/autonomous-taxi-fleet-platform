namespace NovaDrive.Domain.Interfaces;

public interface IPaymentGateway
{
    // Returns a transaction reference string if successful
    Task<string> ProcessPayment(decimal amount, Currency currency);
}