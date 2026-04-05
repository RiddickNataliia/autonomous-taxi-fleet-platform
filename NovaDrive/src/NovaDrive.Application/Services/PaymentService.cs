namespace NovaDrive.Application.Services;

public interface IPaymentService
{
    Task<TransactionResponse> ProcessPayment(ProcessPaymentRequest request, CancellationToken ct = default);
    Task<TransactionResponse> GetTransactionByRide(Guid rideId, CancellationToken ct = default);
}
public sealed class PaymentService : IPaymentService
{
    private readonly ITransactionRepository _transactionRepo;
    private readonly IRideRepository        _rideRepo;
    private readonly IPassengerRepository   _passengerRepo;
    private readonly IPaymentGateway        _gateway;
    private readonly IUnitOfWork            _unitOfWork;

    public PaymentService(
        ITransactionRepository transactionRepo,
        IRideRepository        rideRepo,
        IPassengerRepository   passengerRepo,
        IPaymentGateway        gateway,
        IUnitOfWork            unitOfWork)
    {
        _transactionRepo = transactionRepo;
        _rideRepo        = rideRepo;
        _passengerRepo   = passengerRepo;
        _gateway         = gateway;
        _unitOfWork      = unitOfWork;
    }

    /// <summary>
    /// Creates a Transaction, calls the payment gateway, marks it
    /// successful or failed, then marks the Ride as paid on success.
    /// Everything is committed in a single SaveChanges call.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown if the Ride doesn't exist.</exception>
    public async Task<TransactionResponse> ProcessPayment(ProcessPaymentRequest request, CancellationToken ct = default)
    {
        var ride = await _rideRepo.GetById(request.RideId, ct)
            ?? throw new KeyNotFoundException(RideDomainException.NotFound);

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, ignoreCase: true, out var method)
            || method == PaymentMethod.Unknown)
            throw new UserDomainException(UserDomainException.InvalidPaymentMethod);

        var transaction = new Transaction
        {
            RideId        = ride.Id,
            Amount        = ride.FinalPrice,
            Currency      = Currency.EUR,
            PaymentMethod = method
        };

        await _transactionRepo.Add(transaction, ct);

        try
        {
            var reference = await _gateway.ProcessPayment(transaction.Amount, transaction.Currency);
            transaction.MarkAsSuccessful(reference);
            ride.MarkAsPaid();
        }
        catch
        {
            transaction.MarkAsFailed();
        }

        await _unitOfWork.SaveChanges(ct);
        return transaction.ToResponse();
    }

    /// <summary>
    /// Retrieves the Transaction associated with a given Ride.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown if no transaction is found for the given Ride.</exception>
    public async Task<TransactionResponse> GetTransactionByRide(
        Guid rideId, CancellationToken ct = default)
    {
        var transaction = await _transactionRepo.GetByRideId(rideId, ct)
            ?? throw new KeyNotFoundException("No transaction found for this ride.");

        return transaction.ToResponse();
    }

}
