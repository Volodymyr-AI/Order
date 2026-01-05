using System.Globalization;
using MediatR;
using Order.Application.Common.Exceptions;
using Order.Application.Interfaces;
using Order.Core.BaseModels;

namespace Order.Application.Orders.Queries.GetOrder;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDetailsDto?>
{
    private readonly IOrderRepository _repo;
    private readonly ICurrentUser _currentUser;

    public GetOrderQueryHandler(IOrderRepository repo, ICurrentUser currentUser )
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<OrderDetailsDto?> Handle(GetOrderQuery request, CancellationToken ct)
    {
        if(!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            throw new UnauthorizedAccessException("User is not authenticated");
        
        var order = await _repo.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            throw new NotFoundException($"Order with id: {request.OrderId} not found");
        
        if(order.CustomerId != _currentUser.UserId)
            throw new ForbiddenException("You are not allowed to access this order.");
        
        return new OrderDetailsDto(
            order.Id,
            order.CustomerId,
            order.StoreId,
            order.Status.ToString(),
            order.Total.Currency.Code,
            order.Total.Amount,
            order.Items.Select(i => new OrderItemDto(
                    i.ProductId,
                    i.NameSnapshot,
                    i.UnitPriceSnapshot.Amount,
                    i.UnitPriceSnapshot.Currency.Code,
                    i.Quantity,
                    i.LineTotal.Amount))
                .ToList(),
            order.CreatedAt
            );
    }
}