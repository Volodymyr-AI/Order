using Order.Core.BaseModels;

namespace Order.Application.Interfaces;

public interface ICustomerOrderRepository
{
    Task<CustomerOrder?> GetOrderAsync(Guid orderId, CancellationToken ct);
    Task AddOrderAsync(CustomerOrder order, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct = default); 
}