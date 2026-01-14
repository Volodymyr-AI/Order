using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Interfaces;
using Orders.Persistence.Repositories;

namespace Orders.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<OutboxSaveChangesInterceptor>();
        services.AddDbContext<OrdersDbContext>((sp, opt) =>
        {
            opt.UseNpgsql(connectionString);
            opt.AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>());
        });
        services.AddScoped<IOrderRepository, OrderRepository>();
        return services;
    }
}