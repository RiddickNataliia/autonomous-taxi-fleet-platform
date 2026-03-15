namespace NovaDrive.Domain.ValueObjects;
public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "EUR";

    public Money(decimal amount, string currency = "EUR")
    {
        if (amount < 0) throw new DomainException("Amount cannot be negative.");
        Amount = amount;
        Currency = currency;
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency) throw new DomainException("Currency mismatch.");
        return new Money(a.Amount + b.Amount, a.Currency);
    }
}