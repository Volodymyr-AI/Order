namespace Order.Core.BaseModels;

public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private const int DefaultScale = 2;
    
    public static Money Zero(string currency) => new(0m, currency);

    public Money(decimal amount, string currency)
    {
        if(string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        if (amount < 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

        Currency = Normalize(currency);
        Amount = Round(amount, Currency);
    }

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency); 
    }

    public static Money operator *(Money a, int multiplier)
    {
        if(multiplier < 0) throw new ArgumentOutOfRangeException(nameof(multiplier));
        return new Money(a.Amount * multiplier, a.Currency);
    }

    public static void EnsureSameCurrency(Money a, Money b)
    {
        if (!string.Equals(a.Currency, b.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Currency mismatch: {a.Currency} vs {b.Currency}.");
    }
    private static string Normalize(string c) => (c ?? string.Empty).Trim().ToUpperInvariant();

    private static decimal Round(decimal value, string currency)
    {
        var scale = currency switch
        {
            "JPY" => 0,
            _ => DefaultScale
        };
        
        return Math.Round(value, scale, MidpointRounding.AwayFromZero);
    }
}