using MediatR;
using Order.Application.Common.Exceptions;
using Order.Application.Interfaces;

namespace Order.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Unit>
{
    private readonly IOrderRepository _repo;
    private readonly ICurrentUser _currentUser;

    public CancelOrderCommandHandler(IOrderRepository repo, ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            throw new UnauthorizedAccessException("User is not authenticated");
        
        var order = await _repo.GetByIdAsync(request.OrderId, ct);
        if(order is null)
            throw new NotFoundException($"Order '{request.OrderId}' was not found.");
        if(order.CustomerId != _currentUser.UserId)
            throw new ForbiddenException("You are not allowed to cancel this order.");
        
        order.Cancel();
        await _repo.SaveChangesAsync(ct);
        
        return Unit.Value;
    }
}