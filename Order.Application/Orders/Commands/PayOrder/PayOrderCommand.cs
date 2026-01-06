using MediatR;

namespace Order.Application.Orders.Commands.PayOrder;

public sealed record PayOrderCommand(Guid OrderId) : IRequest;