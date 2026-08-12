namespace CSharpLanguageWorkbench.Domain;

public enum OrderStatus
{
    Pending,
    Paid,
    Cancelled
}

public readonly record struct Money
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money From(decimal amount, string currency)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        return new Money(amount, currency.ToUpperInvariant());
    }

    public static Money operator +(Money left, Money right)
    {
        if (!StringComparer.Ordinal.Equals(left.Currency, right.Currency))
        {
            throw new InvalidOperationException("Money values must use the same currency.");
        }

        return From(left.Amount + right.Amount, left.Currency);
    }
}

public sealed record OrderLine(string Sku, int Quantity, Money UnitPrice)
{
    public Money LineTotal => Money.From(UnitPrice.Amount * Quantity, UnitPrice.Currency);
}

public sealed record Order(
    Guid Id,
    string CustomerEmail,
    OrderStatus Status,
    IReadOnlyList<OrderLine> Lines,
    string? PromotionCode = null);
