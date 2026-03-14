public class DiscountDomainException : Exception
{
    public const string Expired = "This discount code has expired.";
    public const string BelowMinimum = "The ride value is too low for this discount.";
    public const string Inactive = "This discount code is no longer active.";

    public DiscountDomainException(string message) : base(message) { }
}