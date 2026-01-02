using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Interfaces;
using Orders.Persistence.Repositories;

namespace Orders.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<OrdersDbContext>(opt => opt.UseNpgsql(connectionString));
        services.AddScoped<IOrderRepository, OrderRepository>();
        return services;
    }
}