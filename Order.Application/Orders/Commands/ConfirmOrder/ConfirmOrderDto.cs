using Order.Core.BaseModels;

namespace Order.Application.Orders.Commands.ConfirmOrder;

public record ConfirmOrderDto(
    Guid OrderId,
    Guid CustomerId,
    string OrderStatus);