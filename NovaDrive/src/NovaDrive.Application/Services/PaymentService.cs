namespace NovaDrive.Application.Services;

public class PaymentService
{
    private readonly IPaymentGateway _paymentGateway; // Defined in Domain
    private readonly ITransactionRepository _repo;    // Defined in Domain

    public PaymentService(IPaymentGateway paymentGateway, ITransactionRepository repo)
    {
        _paymentGateway = paymentGateway;
        _repo = repo;
    }

    public async Task ProcessRidePaymentAsync(Guid transactionId)
    {
        var transaction = await _repo.GetByIdAsync(transactionId);
        
        try 
        {
            // This calls the "Demo" implementation because of Dependency Injection
            string reference = await _paymentGateway.ProcessPaymentAsync(transaction.Amount, transaction.Currency);
            
            transaction.MarkAsSuccessful(reference);
        }
        catch (Exception)
        {
            transaction.MarkAsFailed();
        }

        await _repo.UpdateAsync(transaction);
    }
}