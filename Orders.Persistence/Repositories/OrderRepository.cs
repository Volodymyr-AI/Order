using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Application.Interfaces;
using Order.Core.BaseModels;

namespace Orders.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _dbContext;

    public OrderRepository(OrdersDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<CustomerOrder?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _dbContext.CustomerOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public void Add(CustomerOrder order) => _dbContext.CustomerOrders.Add(order);

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _dbContext.SaveChangesAsync(ct);
    }
}