using MediatR;

namespace Order.Application.Orders.Commands.ConfirmOrder;

public record ConfirmOrderCommand(Guid OrderId) 
    : IRequest<ConfirmOrderDto>;