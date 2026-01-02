using MediatR;
using Order.Core.BaseModels;

namespace Order.Application.Orders.Queries.GetOrder;

public sealed record GetOrderQuery(
    Guid OrderId) : IRequest<OrderDetailsDto?>;