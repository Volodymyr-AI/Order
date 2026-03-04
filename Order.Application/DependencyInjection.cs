using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Order.Application.Common.Behaviors;
using Order.Application.Orders.Queries.GetOrder;

namespace Order.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // FluentValidation:
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // pipeline behavior:
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        services.Decorate<IRequestHandler<GetOrderQuery, OrderDetailsDto?>, CachedGetOrderQueryHandler>();

        return services;
    }
}