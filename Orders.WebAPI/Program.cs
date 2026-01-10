using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUser, CurrentUser>();

        if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                    options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
        }
        else
        {
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    var key = builder.Configuration["Jwt:Key"];
                    if (string.IsNullOrWhiteSpace(key))
                        throw new InvalidOperationException("Jwt:Key is not configured.");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(1),
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });
        }
        
        builder.Services.AddAuthorization();
        
        builder.Services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();
        builder.Services.AddSingleton<IOutboxPublisher, LoggingOutboxPublisher>();
        builder.Services.AddHostedService<OutboxDispatcherBackgroundService>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Layers injections
        builder.Services.AddApplication();
        if (!builder.Environment.IsEnvironment("Testing") &&
            !builder.Environment.IsEnvironment("JwtTesting"))
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
        
        #if DEBUG
        app.MapGet("/dev/token", () =>
        {
            var userId = Guid.NewGuid();
            var key = builder.Configuration["Jwt:Key"]!;
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), userId });
        });
        #endif
        
        app.MapControllers();

        app.Run();
    }
}

public partial class Program { }