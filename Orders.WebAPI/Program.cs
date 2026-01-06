using Microsoft.EntityFrameworkCore;
using Order.Application;
using Order.Application.Interfaces;
using Order.Core.Outbox;
using Orders.Persistence;
using Orders.Persistence.Repositories;
using Orders.WebAPI.Auth;
using Orders.WebAPI.Middlewares;
using Orders.WebAPI.Workers;
using Scalar.AspNetCore;

namespace Orders.WebAPI;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();
        
        builder.Services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();
        builder.Services.AddSingleton<IOutboxPublisher, LoggingOutboxPublisher>();
        builder.Services.AddHostedService<OutboxDispatcherBackgroundService>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Layers injections
        builder.Services.AddApplication();
        if (!builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddPersistence(builder.Configuration.GetConnectionString("OrdersDb")!);
        }
        else
        {
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        }

        builder.Services.AddTransient<ExceptionHandlingMiddleware>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        
        app.UseHttpsRedirection();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();

        app.Run();
    }
}

public partial class Program { }