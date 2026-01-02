using Order.Application.Orders.Commands.CreateOrder;

namespace Order.Application.Orders.Queries.GetOrder;

public sealed record OrderDetailsDto(
    Guid OrderId,
    Guid CustomerId,
    int StoreId,
    string Status,
    string Currency,
    decimal Total,
    List<OrderItemDto> Items,
    DateTimeOffset CreatedAt
    );