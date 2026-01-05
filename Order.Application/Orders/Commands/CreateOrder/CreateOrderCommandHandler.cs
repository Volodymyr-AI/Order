using MediatR;
using Order.Application.Interfaces;
using Order.Core.BaseModels;

namespace Order.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if(request.CustomerId == Guid.Empty)
            throw new ArgumentException("CustomerId is required.", nameof(request.CustomerId));
        
        if(request.StoreId <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.StoreId), "StoreId must be > 0.");
        
        if(request.Items is null || request.Items.Count == 0)
            throw new InvalidOperationException("Order must contain at least one item.");
        
        //DTO -> Domain
        var items = request.Items.Select(i =>
        {
            if (i.ProductId == Guid.Empty)
                throw new ArgumentException("ProductId is required.");
            if(string.IsNullOrWhiteSpace(i.NameSnapshot))
                throw new ArgumentException("NameSnapshot is required.");
            if(i.Quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(i.Quantity), "Quantity must be > 0.");
            if(string.IsNullOrWhiteSpace(i.CurrencyCode))
                throw new ArgumentException("CurrencyCode is required.");
            if(i.UnitPriceAmount < 0m)
                throw new ArgumentOutOfRangeException(nameof(i.UnitPriceAmount), "Price must be >= 0.");
            
            var currency = Currency.FromCode(i.CurrencyCode);
            var money = new Money(i.UnitPriceAmount, currency);
            
            return new OrderItem(i.ProductId, i.NameSnapshot, money,  i.Quantity);
        }).ToList();

        var order = CustomerOrder.Create(request.CustomerId, request.StoreId, items);
        
         _orderRepository.Add(order);
        await _orderRepository.SaveChangesAsync(cancellationToken);
        
        return order.Id;
    }
}