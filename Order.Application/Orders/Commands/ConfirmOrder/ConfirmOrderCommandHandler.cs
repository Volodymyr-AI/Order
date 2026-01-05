using MediatR;
using Order.Application.Common.Exceptions;
using Order.Application.Interfaces;

namespace Order.Application.Orders.Commands.ConfirmOrder;

public sealed class ConfirmOrderCommandHandler : IRequestHandler<ConfirmOrderCommand, ConfirmOrderDto>
{
    private readonly IOrderRepository _repo;
    private readonly ICurrentUser _currentUser;
    
    public ConfirmOrderCommandHandler(IOrderRepository repo,  ICurrentUser currentUser)
    {
        _repo = repo;
        _currentUser = currentUser;
    }
    
    public async Task<ConfirmOrderDto> Handle(ConfirmOrderCommand request, CancellationToken ct)
    {
        if(!_currentUser.IsAuthenticated || _currentUser.UserId == Guid.Empty)
            throw new UnauthorizedAccessException("User is not authenticated");

        var order = await _repo.GetByIdAsync(request.OrderId, ct);
        if(order is null)
            throw new NotFoundException($"Order '{request.OrderId}' was not found.");
        
        if(order.CustomerId != _currentUser.UserId)
            throw new ForbiddenException("You are not allowed to confirm this order.");
        
        order.Confirm();

        await _repo.SaveChangesAsync(ct);
        return new ConfirmOrderDto(
            order.Id,
            order.CustomerId,
            order.Status.ToString());
    }
}