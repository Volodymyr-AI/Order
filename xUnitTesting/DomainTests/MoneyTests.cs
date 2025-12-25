using Order.Core.BaseModels;

namespace xUnitTesting.DomainTests;

public class MoneyTests
{
    [Fact]
    public void Money_rounds_away_from_zero()
    {
        var money = new Money(10.005m, Currency.FromCode("USD"));
        Assert.Equal(10.01m, money.Amount);
    } 
}