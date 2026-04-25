namespace NovaDrive.Tests.UnitTests;

public class TransactionTests
{
    // helpers
    private static readonly Guid ValidRideId = Guid.NewGuid();
    private const string TransactionReference = "PAYMENT12345";

    private static Transaction CreateTransaction() => Transaction.Create(
        rideId:   ValidRideId,
        amount:   25.50m,
        currency: Currency.EUR,
        method:   PaymentMethod.CreditCard);

    // Create

    [Fact]
    public void Create_WithValidData_SetsPendingStatus()
    {
        var transaction = CreateTransaction();
        Assert.Equal(TransactionStatus.Pending, transaction.Status);
    }

    [Fact]
    public void Create_WithValidData_HasNullBankReference()
    {
        var transaction = CreateTransaction();
        Assert.Null(transaction.BankReference);
    }

    [Fact]
    public void Create_WithEmptyRideId_Throws()
    {
        var ex = Assert.Throws<TransactionDomainException>(() =>
            Transaction.Create(Guid.Empty, 25.50m, Currency.EUR, PaymentMethod.CreditCard));
        Assert.Equal(TransactionDomainException.InvalidRideId, ex.Message);
    }

    [Fact]
    public void Create_WithZeroAmount_Throws()
    {
        var ex = Assert.Throws<TransactionDomainException>(() =>
            Transaction.Create(ValidRideId, 0m, Currency.EUR, PaymentMethod.CreditCard));
        Assert.Equal(TransactionDomainException.InvalidAmount, ex.Message);
    }

    [Fact]
    public void Create_WithNegativeAmount_Throws()
    {
        var ex = Assert.Throws<TransactionDomainException>(() =>
            Transaction.Create(ValidRideId, -10m, Currency.EUR, PaymentMethod.CreditCard));
        Assert.Equal(TransactionDomainException.InvalidAmount, ex.Message);
    }

    [Fact]
    public void Create_WithUnknownCurrency_Throws()
    {
        var ex = Assert.Throws<TransactionDomainException>(() =>
            Transaction.Create(ValidRideId, 25.50m, Currency.Unknown, PaymentMethod.CreditCard));
        Assert.Equal(TransactionDomainException.InvalidCurrency, ex.Message);
    }

    [Fact]
    public void Create_WithUnknownPaymentMethod_Throws()
    {
        var ex = Assert.Throws<TransactionDomainException>(() =>
            Transaction.Create(ValidRideId, 25.50m, Currency.EUR, PaymentMethod.Unknown));
        Assert.Equal(TransactionDomainException.InvalidPaymentMethod, ex.Message);
    }

    //  MarkAsSuccessful

    [Fact]
    public void MarkAsSuccessful_WithValidReference_SetsStatusAndReference()
    {
        var transaction = CreateTransaction();

        transaction.MarkAsSuccessful(TransactionReference);

        Assert.Equal(TransactionStatus.Successful, transaction.Status);
        Assert.Equal(TransactionReference, transaction.BankReference);
    }

    [Fact]
    public void MarkAsSuccessful_WhenNotPending_Throws()
    {
        var transaction = CreateTransaction();
        transaction.MarkAsSuccessful(TransactionReference);

        var ex = Assert.Throws<TransactionDomainException>(() =>
            transaction.MarkAsSuccessful("REF456"));
        Assert.Equal(TransactionDomainException.NotPending, ex.Message);
    }

    [Fact]
    public void MarkAsSuccessful_WithEmptyReference_Throws()
    {
        var transaction = CreateTransaction();

        var ex = Assert.Throws<TransactionDomainException>(() =>
            transaction.MarkAsSuccessful(string.Empty));
        Assert.Equal(TransactionDomainException.NoReference, ex.Message);
    }

    [Fact]
    public void MarkAsSuccessful_WhenAlreadyFailed_Throws()
    {
        var transaction = CreateTransaction();
        transaction.MarkAsFailed();

        var ex = Assert.Throws<TransactionDomainException>(() =>
            transaction.MarkAsSuccessful(TransactionReference));
        Assert.Equal(TransactionDomainException.NotPending, ex.Message);
    }

    // MarkAsFailed

    [Fact]
    public void MarkAsFailed_WhenPending_SetsStatusToFailed()
    {
        var transaction = CreateTransaction();

        transaction.MarkAsFailed();

        Assert.Equal(TransactionStatus.Failed, transaction.Status);
    }

    [Fact]
    public void MarkAsFailed_WhenNotPending_Throws()
    {
        var transaction = CreateTransaction();
        transaction.MarkAsSuccessful(TransactionReference);

        var ex = Assert.Throws<TransactionDomainException>(() =>
            transaction.MarkAsFailed());
        Assert.Equal(TransactionDomainException.CannotFail, ex.Message);
    }
}