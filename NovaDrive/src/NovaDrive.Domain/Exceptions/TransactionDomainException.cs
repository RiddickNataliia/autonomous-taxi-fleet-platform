namespace NovaDrive.Domain.Exceptions;

public class TransactionDomainException : Exception
{
    public const string NotPending = "Only pending transactions can be marked as successful.";
    public const string NoReference = "A bank reference is required for successful transactions.";
    public const string CannotFail = "Only pending transactions can be failed.";
    public const string InvalidReference = "The bank reference provided is invalid or empty.";
    public TransactionDomainException(string message) : base(message)
    {
    }
}       
       