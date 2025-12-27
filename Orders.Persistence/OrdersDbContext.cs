using Microsoft.EntityFrameworkCore;
using Order.Core.BaseModels;
using Orders.Persistence.Configurations;

namespace Orders.Persistence;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}