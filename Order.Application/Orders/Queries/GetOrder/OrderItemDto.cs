namespace Order.Application.Orders.Queries.GetOrder;

public sealed record OrderItemDto(
    Guid ProductId,
    string NameSnapshot,
    decimal UnitPriceAmount,
    string CurrencyCode,
    int Quantity,
    decimal LineTotal
    );