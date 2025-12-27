using Microsoft.EntityFrameworkCore;
using Orders.Persistence;

namespace Orders.WebAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddDbContext<OrdersDbContext>(opt => 
            opt.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDb")));
        
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection(); 

        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}