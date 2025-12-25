using Order.Core.BaseModels;
using Order.Core.Event;
using Xunit.Abstractions;

namespace xUnitTesting.DomainTests;

public class CustomerOrderTests
{
    [Fact]
    public void Total_returns_correct_amount()
    {
        var order = new CustomerOrder(Guid.NewGuid(), Guid.NewGuid(), 1);
        
        order.AddItem(new OrderItem(Guid.NewGuid(), "Item A", new Money(10m, Currency.FromCode("USD")), 2));
        order.AddItem(new OrderItem(Guid.NewGuid(), "Item B", new Money(5m, Currency.FromCode("USD")), 3));
        
        Assert.Equal(35m, order.Total.Amount);
        Assert.Equal(Currency.FromCode("USD"), order.Total.Currency);
    }

    [Fact]
    public void AddItem_fails_when_items_have_different_currencies()
    {
        var order = new CustomerOrder(Guid.NewGuid(), Guid.NewGuid(), 1);
        order.AddItem(new OrderItem(Guid.NewGuid(), "Item A", new Money(5m, Currency.FromCode("USD")), 1));
        Assert.Throws<InvalidOperationException>(() => 
            order.AddItem(new OrderItem(Guid.NewGuid(), "Item B", new Money(10m, Currency.FromCode("EUR")), 1)));
    }

    [Fact]
    public void LineTotal_rounds_away_from_zero()
    {
        var price = new Money(0.335m, Currency.FromCode("USD"));
        var line = price * 3; // 0.34 * 3 = 1.02
        Assert.Equal(1.02m, line.Amount);
    }

    [Fact]
    public void AddItem_throws_after_confirm()
    {
        var order = new CustomerOrder(Guid.NewGuid(), Guid.NewGuid(), 1);
        order.AddItem(new OrderItem(Guid.NewGuid(), "Item A", new Money(5m, Currency.FromCode("USD")), 1));
        
        order.Confirm();
        
        Assert.Throws<InvalidOperationException>(() => order.AddItem(new OrderItem(Guid.NewGuid(), "Item B", new Money(1m, Currency.FromCode("USD")), 1)));
    }

    [Fact]
    public void Pay_throws_before_confirm()
    {
        var order = new CustomerOrder(Guid.NewGuid(), Guid.NewGuid(), 1);
        order.AddItem(new OrderItem(Guid.NewGuid(), "Item A", new Money(5m, Currency.FromCode("USD")), 1));
        
        Assert.Throws<InvalidOperationException>(() => order.Pay());
    }

    [Fact]
    public void Total_uses_money_rounding_away_from_zero()
    {
        var order = new CustomerOrder(Guid.NewGuid(), Guid.NewGuid(), 1);
        
        // 10.005 => 10.01
        order.AddItem(new OrderItem(Guid.NewGuid(), "Item A", new Money(10.005m, Currency.FromCode("USD")), 1));
        
        Assert.Equal(10.01m, order.Total.Amount);
    }

    [Fact]
    public void RemoveItem_removes_item_and_recalculates_total()
    {
        var order = new CustomerOrder(Guid.NewGuid(),  Guid.NewGuid(), 1);
        
        var aId = Guid.NewGuid();
        var bId = Guid.NewGuid();
        
        order.AddItem(new OrderItem(aId, "A", new Money(10m, Currency.FromCode("USD")), 2)); // 20
        order.AddItem(new OrderItem(bId, "B", new Money(5m, Currency.FromCode("USD")), 3)); // 15
        Assert.Equal(35m, order.Total.Amount);
        
        order.RemoveItem(bId);
        Assert.Equal(20m, order.Total.Amount);
        Assert.Single(order.Items);
    }

    [Fact]
    public void RemoveItem_throws_when_item_not_found()
    {
        var order = new CustomerOrder(Guid.NewGuid(), Guid.NewGuid(), 1);
        order.AddItem(new OrderItem(Guid.NewGuid(), "A", new Money(10m, Currency.FromCode("USD")), 1));
        
        Assert.Throws<InvalidOperationException>(() => order.RemoveItem(Guid.NewGuid()));
    }

    [Fact]
    public void ChangeQuantity_updates_total()
    {
        var order = new CustomerOrder(Guid.NewGuid(), Guid.NewGuid(), 1);
        
        var aId = Guid.NewGuid();
        order.AddItem(new OrderItem(aId, "A", new Money(10m, Currency.FromCode("USD")), 2)); // 20
        
        order.ChangeQuantity(aId, 5); // 50
        
        Assert.Equal(50m, order.Total.Amount);
    }

    [Fact]
    public void ChangeQuantity_throws_after_confirm()
    {
        var order = new CustomerOrder(Guid.NewGuid(), Guid.NewGuid(), 1);
        
        var aId = Guid.NewGuid();
        order.AddItem(new OrderItem(aId, "A", new Money(10m, Currency.FromCode("USD")), 1));
        order.Confirm();
        
        Assert.Throws<InvalidOperationException>(() => order.ChangeQuantity(aId, 2));
    }

    [Fact]
    public void Confirm_adds_domain_event()
    {
        var order = new CustomerOrder(Guid.NewGuid(), Guid.NewGuid(), 1);
        order.AddItem(new OrderItem(Guid.NewGuid(), "Item A", new Money(10m, Currency.FromCode("USD")), 1));

        order.Confirm();
        
        var ev = Assert.Single(order.DomainEvents);
        var confirmed = Assert.IsType<CustomerOrderConfirmed>(ev);

        Assert.Equal(order.Id, confirmed.OrderId);
        Assert.Equal(order.CustomerId, confirmed.CustomerId);
        Assert.Equal(order.Total, confirmed.Total);
    }

    [Fact]
    public void Pay_adds_domain_event()
    {
        var order = new CustomerOrder(Guid.NewGuid(), Guid.NewGuid(), 1);
        order.AddItem(new OrderItem(Guid.NewGuid(), "Item A", new Money(10m, Currency.FromCode("USD")), 1));
        
        order.Confirm();
        order.ClearDomainEvents();

        order.Pay();
        
        var ev = Assert.Single(order.DomainEvents);
        var paid = Assert.IsType<CustomerOrderPaid>(ev);
        
        Assert.Equal(order.Id, paid.OrderId);
        Assert.Equal(order.CustomerId, paid.CustomerId);
        Assert.Equal(order.Total, paid.Total);
    }
}