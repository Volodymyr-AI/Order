using MediatR;

namespace Order.Application.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) 
    : IRequest<Unit>;