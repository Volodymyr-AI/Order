using MediatR;
using Order.Application.Common.Exceptions;
using Order.Application.Interfaces;
using Order.Core.Outbox;

namespace Order.Application.Orders.Commands.PayOrder;

public sealed class PayOrderCommandHandler : IRequestHandler<PayOrderCommand>
{
    private readonly IOrderRepository _repo;
    private readonly ICurrentUser _currentUser;
    public PayOrderCommandHandler(IOrderRepository repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task Handle(PayOrderCommand request, CancellationToken ct)
    {
        if(!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            throw new UnauthorizedAccessException("User is not authenticated.");
        
        var order = await _repo.GetByIdAsync(request.OrderId, ct);
        
        if(order is null)
            throw new NotFoundException($"Order '{request.OrderId}' was not found.");
        
        if(order.CustomerId != _currentUser.UserId)
            throw new ForbiddenException("You are not allowed to pay this order.");
        
        order.Pay();
        await _repo.SaveChangesAsync(ct);
    }
}