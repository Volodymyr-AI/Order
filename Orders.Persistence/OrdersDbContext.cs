using Microsoft.EntityFrameworkCore;
using Order.Application.Interfaces;
using Order.Core.BaseModels;
using Order.Core.Outbox;
using Orders.Persistence.Configurations;

namespace Orders.Persistence;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>(); 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrdersDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}