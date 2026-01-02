using System.Net;
using System.Net.Http.Json;

namespace xUnitTesting.PresentationTests;

public sealed class OrdersEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_orders_returns_201_and_orderId()
    {
        var request = new
        {
            customerId = Guid.NewGuid(),
            storeId = 1,
            items = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    nameSnapshot = "Item A",
                    unitPriceAmount = 10m,
                    currencyCode = "USD",
                    quantity = 2
                },
                new
                {
                    productId = Guid.NewGuid(),
                    nameSnapshot = "Item B",
                    unitPriceAmount = 5m,
                    currencyCode = "USD",
                    quantity = 3
                }
            }
        };
        
        var response = await _client.PostAsJsonAsync("/api/orders", request);  
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.OrderId);
        
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task POST_orders_with_empty_items_returns_400()
    {
        var request = new
        {
            customerId = Guid.NewGuid(),
            storeId = 1,
            items = Array.Empty<object>()
        };
        
        var response = await _client.PostAsJsonAsync("/api/orders", request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    

    private sealed record CreateOrderResponse(Guid OrderId);
}