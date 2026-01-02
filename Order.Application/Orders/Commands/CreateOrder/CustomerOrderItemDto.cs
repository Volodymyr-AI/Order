namespace Order.Application.Orders.Commands.CreateOrder;

public record CustomerOrderItemDto(
    Guid ProductId,
    string NameSnapshot,
    decimal UnitPriceAmount,
    string CurrencyCode,
    int Quantity
    );