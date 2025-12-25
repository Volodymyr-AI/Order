namespace Order.Core.BaseModels;

public readonly record struct Currency(string Code, int DecimalPlaces)
{
    public bool IsValid => !string.IsNullOrWhiteSpace(Code);

    private static readonly Dictionary<string, int> KnownCurrencies = new()
    {
        ["USD"] = 2,
        ["EUR"] = 2,
        ["GBP"] = 2,
        ["JPY"] = 0,
        ["UAH"] = 2
    };

    public static Currency FromCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code is required.", nameof(code));
        
        var normalized = code.Trim().ToUpperInvariant();    
        
        if (!KnownCurrencies.TryGetValue(normalized, out var places))
            throw new ArgumentException($"Unknown currency: {code}");
        
        return new Currency(normalized, places);
    }
}