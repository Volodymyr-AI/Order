using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Order.Core.BaseModels;

namespace Orders.Persistence.Converters;

public sealed class CurrencyCodeConverter : ValueConverter<Currency, string>
{
    public CurrencyCodeConverter() : base(c => c.Code, s => Currency.FromCode(s))
    {
    }
}