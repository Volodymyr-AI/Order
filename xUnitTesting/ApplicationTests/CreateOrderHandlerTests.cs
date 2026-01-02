using NSubstitute;
using Order.Application.Interfaces;
using Order.Application.Orders.Commands.CreateOrder;
using Order.Core.BaseModels;

namespace xUnitTesting.ApplicationTests;

public class CreateOrderHandlerTests
{
    [Fact]
    public async Task Handler_creates_order_and_returns_id()
    {
        //Arrange
        var repo = Substitute.For<IOrderRepository>();
        var handler = new CreateOrderCommandHandler(repo);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            1,
            new[]
            {
                new CustomerOrderItemDto(
                    Guid.NewGuid(),
                    "Item A",
                    10m,
                    "USD",
                    2
                )
            });
        
        // Act
        var id = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        repo.Received(1).Add(Arg.Any<CustomerOrder>());
        await repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Handler_drops_when_items_empty()
    {
        var repo = Substitute.For<IOrderRepository>();
        var handler = new CreateOrderCommandHandler(repo);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            1,
            Array.Empty<CustomerOrderItemDto>());
        
        await repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handler_saves_right_total_in_order()
    {
        var repo = Substitute.For<IOrderRepository>();
        var handler = new CreateOrderCommandHandler(repo);
        
        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            1,
            new[]
            {
                new CustomerOrderItemDto(
                    Guid.NewGuid(),
                    "Item A",
                    10m,
                    "USD",
                    2
                ),
                new CustomerOrderItemDto(
                    Guid.NewGuid(),
                    "Item B",
                    5m,
                    "USD",
                    3)
            });
        
        //Act
        var id = await handler.Handle(command, CancellationToken.None);
        //Assert
        repo.Received(1).Add(Arg.Is<CustomerOrder>(o => 
            o.Total.Amount == 35m &&
            o.Total.Currency.Code == "USD" &&
            o.Items.Count == 2 &&
            o.Status == OrderStatus.Draft
            ));
        await repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Handler_throws_when_currencies_mismatch()
    {
        var repo = Substitute.For<IOrderRepository>();
        var handler = new CreateOrderCommandHandler(repo);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            1,
            new[]
            {
                new CustomerOrderItemDto(
                    Guid.NewGuid(),
                    "Item A",
                    10m,
                    "USD",
                    2
                ),
                new CustomerOrderItemDto(
                    Guid.NewGuid(),
                    "Item B",
                    5m,
                    "EUR",
                    3)
            });
        
        // Act + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        repo.DidNotReceive().Add(Arg.Any<CustomerOrder>());
        await repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>()); 
    }

    [Fact]
    public async Task Handler_throws_when_quantity_is_not_positive()
    {
        var repo = Substitute.For<IOrderRepository>();
        var handler = new CreateOrderCommandHandler(repo);

        var command = new CreateOrderCommand(
            Guid.NewGuid(),
            1,
            [
                new CustomerOrderItemDto(
                    Guid.NewGuid(),
                    "Item A",
                    10m,
                    "USD",
                    0
                )
            ]);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => handler.Handle(command, CancellationToken.None));
        repo.DidNotReceive().Add(Arg.Any<CustomerOrder>());
        await repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}