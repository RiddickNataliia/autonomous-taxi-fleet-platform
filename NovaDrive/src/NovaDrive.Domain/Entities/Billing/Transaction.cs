namespace NovaDrive.Domain.Entities;

public class Transaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid RideId { get; init; } 
    public decimal Amount { get; init; } 
    public PaymentMethod PaymentMethod { get; init; } = PaymentMethod.Unknown; 
    public Currency Currency { get; init; } = Currency.EUR;

    public TransactionStatus Status { get; private set; } = TransactionStatus.Pending;
    
    public string? BankReference { get; private set; } 
    
    public DateTimeOffset PaymentDate { get; init; } = DateTimeOffset.UtcNow;

    private Transaction() { }

    public static Transaction Create(Guid rideId, decimal amount, Currency currency, PaymentMethod method)
    {
        if (rideId == Guid.Empty)
            throw new TransactionDomainException(TransactionDomainException.InvalidRideId);

        if (amount <= 0)
            throw new TransactionDomainException(TransactionDomainException.InvalidAmount);

        if (currency == Currency.Unknown)
            throw new TransactionDomainException(TransactionDomainException.InvalidCurrency);

        if (method == PaymentMethod.Unknown)
            throw new TransactionDomainException(TransactionDomainException.InvalidPaymentMethod);

        return new Transaction
        {
            RideId        = rideId,
            Amount        = amount,
            Currency      = currency,
            PaymentMethod = method
        };
    }

    /// <summary>
    /// Marks the transaction as successful when payment is confirmed by the gateway.
    /// </summary>
    /// <param name="reference">The unique reference ID from the bank.</param>
    /// <exception cref="TransactionDomainException">Thrown if transaction satus is not Pending or bank reference is null or empty string</exception>
    public void MarkAsSuccessful(string reference)
    {
        if (Status != TransactionStatus.Pending)
            throw new TransactionDomainException(TransactionDomainException.NotPending);

        if (string.IsNullOrWhiteSpace(reference))
            throw new TransactionDomainException(TransactionDomainException.NoReference);

        BankReference = reference;
        Status = TransactionStatus.Successful;
    }

    /// <summary>
    /// Marks the transaction as failed.
    /// </summary>
    /// <exception cref="TransactionDomainException">Thrown if transaction satus is not Pending</exception>
    public void MarkAsFailed()
    {
        if (Status != TransactionStatus.Pending)
            throw new TransactionDomainException(TransactionDomainException.CannotFail);

        Status = TransactionStatus.Failed;
    }
}