using Order.Core.BaseModels;

namespace Order.Application.Interfaces;

public interface IOrderRepository
{
    Task<CustomerOrder?> GetByIdAsync(Guid orderId, CancellationToken ct = default);
    void Add(CustomerOrder order); 
    Task SaveChangesAsync(CancellationToken ct = default);
}