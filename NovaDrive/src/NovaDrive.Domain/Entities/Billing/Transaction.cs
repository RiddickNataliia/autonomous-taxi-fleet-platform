namespace NovaDrive.Domain.Entities;

public class Transaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid RideId { get; init; } 
    public decimal Amount { get; init; } 
    public Currency Currency { get; init; } = Currency.EUR;

    public TransactionStatus Status { get; private set; } = TransactionStatus.Pending;
    
    public string? BankReference { get; private set; } 
    
    public DateTimeOffset PaymentDate { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Marks the transaction as successful when payment is confirmed by the gateway.
    /// </summary>
    /// <param name="reference">The unique reference ID from the bank.</param>
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
    public void MarkAsFailed()
    {
        if (Status != TransactionStatus.Pending)
            throw new TransactionDomainException(TransactionDomainException.CannotFail);

        Status = TransactionStatus.Failed;
    }
}