namespace Order.Core.BaseModels;

public readonly record struct Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if(!currency.IsValid) 
            throw new ArgumentException("Currency is required.", nameof(currency));
        
        if (amount < 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount cannot be negative.");

        Currency = currency;
        Amount = Math.Round(amount, currency.DecimalPlaces, MidpointRounding.AwayFromZero);
    }

    public static Money Zero(Currency currency)
    {
        if(!currency.IsValid)
            throw new ArgumentException("Currency is required.", nameof(currency));
        
        return new Money(decimal.Zero, currency);
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
        if(!a.Currency.IsValid || !b.Currency.IsValid)
            throw new InvalidOperationException("Currency is not set.");
        
        if (!string.Equals(a.Currency.Code, b.Currency.Code, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Currency mismatch: {a.Currency} vs {b.Currency}.");
    }
}