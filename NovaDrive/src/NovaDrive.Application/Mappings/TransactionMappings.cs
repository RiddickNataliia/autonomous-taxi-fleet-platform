namespace NovaDrive.Application.Mappings;

public static class TransactionMappings
{
    public static TransactionResponse ToResponse(this Transaction t) => new(
        TransactionId: t.Id,
        RideId:        t.RideId,
        Amount:        t.Amount,
        Currency:      t.Currency.ToString(),
        PaymentMethod: t.PaymentMethod.ToString(),
        Status:        t.Status.ToString(),
        BankReference: t.BankReference,
        PaymentDate:   t.PaymentDate);
}