namespace NovaDrive.Tests.UnitTests;

public class TransactionTests
{
    //helpers:
    private static Transaction CreateTransaction() => new()
    {
        RideId = Guid.NewGuid(),
    };

    string transactionReference = "PAYMENT12345";

    [Fact]
    public void NewTransaction_HasPendingStatus()
    {
        var transaction = CreateTransaction();
        Assert.Equal(TransactionStatus.Pending, transaction.Status);
    }

    [Fact]
    public void MarkAsSuccessful_WithValidReferenceAndPendingStatus_SetsStatusToSuccessful()
    {
        var transaction = CreateTransaction();
        var reference = transactionReference;
        
        transaction.MarkAsSuccessful(reference);
        
        Assert.Equal(TransactionStatus.Successful, transaction.Status);
        Assert.Equal(reference, transaction.BankReference);
    }


    [Fact]
    public void MarkAsSuccessful_WhenNotPending_Throws()
    {
        var transaction = CreateTransaction();
        transaction.MarkAsSuccessful(transactionReference);
        
        var ex = Assert.Throws<TransactionDomainException>(() => transaction.MarkAsSuccessful("REF456"));
        Assert.Equal(TransactionDomainException.NotPending, ex.Message);
    }

    [Fact]
    public void MarkAsSuccessful_WithEmptyReference_Throws()
    {
        var transaction = CreateTransaction();
        
        var ex = Assert.Throws<TransactionDomainException>(() => transaction.MarkAsSuccessful(string.Empty));
        Assert.Equal(TransactionDomainException.NoReference, ex.Message);
    }

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
        transaction.MarkAsSuccessful(transactionReference);
        
        var ex = Assert.Throws<TransactionDomainException>(() => transaction.MarkAsFailed());
        Assert.Equal(TransactionDomainException.CannotFail, ex.Message);
    }
}
