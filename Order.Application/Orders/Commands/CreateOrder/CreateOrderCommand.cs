using MediatR;

namespace Order.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid CustomerId,
    int StoreId,
    IReadOnlyList<CustomerOrderItemDto> Items) : IRequest<Guid>;