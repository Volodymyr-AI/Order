using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orders.Persistence;
using Orders.WebAPI;

namespace xUnitTesting.PresentationTests.Additional;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); 
        
        builder.ConfigureServices(services =>
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            
            services.RemoveAll(typeof(DbContextOptions<OrdersDbContext>));
            services.RemoveAll(typeof(OrdersDbContext));

            services.AddScoped<OutboxSaveChangesInterceptor>();
            services.AddDbContext<OrdersDbContext>((sp, opt) =>
            {
                opt.UseSqlite(_connection);
                opt.AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>());
            });

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}