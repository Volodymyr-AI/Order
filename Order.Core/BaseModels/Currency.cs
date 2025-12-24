namespace Order.Core.BaseModels;

public class Currency
{
    public string Code { get; }
    public int DecimalPlaces { get; }

    public static readonly Currency USD = new("USD", 2);
    public static readonly Currency EUR = new("EUR", 2);
    public static readonly Currency JPY = new("JPY", 0);

    private static readonly Dictionary<string, int> KnownCurrencies = new()
    {
        ["USD"] = 2,
        ["EUR"] = 2,
        ["GBP"] = 2,
        ["JPY"] = 0,
        ["UAH"] = 2
    };

    public Currency(string code, int decimalPlaces)
    {
        if(string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code is required.", nameof(code));
        
        Code = code;
        DecimalPlaces = decimalPlaces;
    }

    public static Currency FromCode(string code)
    {
        var normalized = code.ToUpperInvariant();
        if (!KnownCurrencies.TryGetValue(normalized, out var places))
            throw new ArgumentException($"Unknown currency: {code}");
        
        return new Currency(normalized, places);
    }
}