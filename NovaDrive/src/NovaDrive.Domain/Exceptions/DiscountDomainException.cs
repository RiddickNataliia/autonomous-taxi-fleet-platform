namespace NovaDrive.Domain.Exceptions;
public class DiscountDomainException : DomainException
{
    public const string Expired = "This discount code has expired.";
    public const string BelowMinimum = "The ride value is too low for this discount.";
    public const string Inactive = "This discount code is no longer active.";
    public const string InvalidCode    = "A valid code string is required.";
    public const string InvalidType    = "A valid discount type is required.";
    public const string InvalidValue   = "Discount value must be greater than zero.";
    public const string AlreadyExpired = "Expiration date must be in the future.";
    public const string CodeAlreadyExists = "A discount code with this name already exists.";
    public DiscountDomainException(string message) : base(message) { }
}