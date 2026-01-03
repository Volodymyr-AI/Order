using System.Net;
using System.Net.Http.Json;
using Order.Application.Orders.Queries.GetOrder;

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
    
    [Fact]
    public async Task POST_orders_with_zero_quantity_returns_400()
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
                    quantity = 0
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
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task POST_orders_with_negative_quantity_returns_400()
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
                    quantity = 1
                },
                new
                {
                    productId = Guid.NewGuid(),
                    nameSnapshot = "Item B",
                    unitPriceAmount = 5m,
                    currencyCode = "USD",
                    quantity = -1
                }
            }
        };
        
        var response = await _client.PostAsJsonAsync("/api/orders", request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_order_by_id_returns_200_for_owner()
    {
        var userId = Guid.NewGuid();

        var createRequest = new
        {
            customerId = userId,
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
                }
            }
        };

        using var createHttp = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = JsonContent.Create(createRequest)
        };
        createHttp.Headers.Add("x-test-user", userId.ToString());
        
        var createResponse = await _client.SendAsync(createHttp);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.OrderId);
        
        using var getHttp = new HttpRequestMessage(HttpMethod.Get, $"/api/orders/{created.OrderId}");
        getHttp.Headers.Add("x-test-user", userId.ToString());
        
        var getResponse = await _client.SendAsync(getHttp);
        
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var dto = await getResponse.Content.ReadFromJsonAsync<OrderDetailsDto>();
        Assert.NotNull(dto);
        Assert.Equal(created.OrderId, dto!.OrderId);
        Assert.Equal(userId, dto.CustomerId);
        Assert.Equal(1, dto.StoreId);
        Assert.True(dto.Items.Count > 0);
    }

    [Fact]
    public async Task GET_order_by_id_returns_403_for_wrong_owner()
    {
        var userId = Guid.NewGuid();
        var wrongUserId = Guid.NewGuid();

        var createRequest = new
        {
            customerId = userId,
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
                }
            }
        };

        using var createHttp = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = JsonContent.Create(createRequest)
        };
        createHttp.Headers.Add("x-test-user", userId.ToString());
        
        var createResponse = await _client.SendAsync(createHttp);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.OrderId);
        
        using var getHttp = new HttpRequestMessage(HttpMethod.Get, $"/api/orders/{created.OrderId}");
        getHttp.Headers.Add("x-test-user", wrongUserId.ToString());
        
        var getResponse = await _client.SendAsync(getHttp);
        
        Assert.Equal(HttpStatusCode.Forbidden, getResponse.StatusCode);
    }
    private sealed record CreateOrderResponse(Guid OrderId);
}