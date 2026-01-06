using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Order.Application.Orders.Commands.ConfirmOrder;
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
    public async Task POST_orders_with_no_storeId_returns_400()
    {
        var request = new
        {
            customerId = Guid.NewGuid(),
            storeId = 0,
            items = new[]
            {
                new
                {
                    productId = Guid.NewGuid(),
                    nameSnapshot = "Item A",
                    unitPriceAmount = 10m,
                    currencyCode = "USD",
                    quantity = 1
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

    [Fact]
    public async Task POST_confirm_order_as_owner_returns_200_and_status_confirmed()
    {
        var userId = Guid.NewGuid();
        
        var request = new
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
        var createResponse = await _client.PostAsJsonAsync("/api/orders", request);  
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>();
        Assert.NotNull(created);

        var confirm = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/orders/{created.OrderId}/confirm");
        confirm.Headers.Add("x-test-user", userId.ToString());
        
        var confirmedResponse = 
            await _client.SendAsync(confirm);
        Assert.Equal(HttpStatusCode.OK, confirmedResponse.StatusCode);
        
        var dto = await confirmedResponse.Content.ReadFromJsonAsync<ConfirmOrderDto>();
        Assert.NotNull(dto);
        Assert.Equal(created.OrderId, dto!.OrderId);
        Assert.Equal("Confirmed", dto.OrderStatus);
    }

    [Fact]
    public async Task POST_confirm_as_other_user_returns_403()
    {
        var wrongUserId = Guid.NewGuid();
        
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
                }
            }
        };
        var createResponse = await _client.PostAsJsonAsync("/api/orders", request);  
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>();
        Assert.NotNull(created);
        
        var confirm = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/orders/{created.OrderId}/confirm");
        confirm.Headers.Add("x-test-user", wrongUserId.ToString());
        
        var confirmedResponse = 
            await _client.SendAsync(confirm);
        
        Assert.Equal(HttpStatusCode.Forbidden, confirmedResponse.StatusCode);
    }

    [Fact]
    public async Task Confirm_non_existing_order_returns_404()
    {
        var confirm = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/orders/{Guid.NewGuid()}/confirm");
        confirm.Headers.Add("x-test-user", Guid.NewGuid().ToString());
        
        var confirmedResponse = await _client.SendAsync(confirm);
        
        Assert.Equal(HttpStatusCode.NotFound, confirmedResponse.StatusCode);
    }
    
    [Fact]
    public async Task Confirm_twice_returns_400()
    {
        var userId = Guid.NewGuid();
        
        var request = new
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

        using var create = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = JsonContent.Create(request)
        };
        create.Headers.Add("x-test-user", userId.ToString());
        
        var createResponse = await _client.SendAsync(create);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var created = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>();
        Assert.NotNull(created);
        
        // confirm #1
        using var confirm1 = new HttpRequestMessage(
            HttpMethod.Post, $"/api/orders/{created!.OrderId}/confirm");
        confirm1.Headers.Add("x-test-user", userId.ToString());

        var r1 = await _client.SendAsync(confirm1);
        Assert.Equal(HttpStatusCode.OK, r1.StatusCode);
        
        // confirm #2
        using var confirm2 = new HttpRequestMessage(
            HttpMethod.Post, $"/api/orders/{created!.OrderId}/confirm");
        confirm2.Headers.Add("x-test-user", userId.ToString());

        var r2 = await _client.SendAsync(confirm2);
        
        Assert.Equal(HttpStatusCode.BadRequest, r2.StatusCode);
    }
    
     [Fact]
    public async Task POST_pay_after_confirm_returns_200()
    {
        var userId = Guid.NewGuid();

        var orderId = await CreateOrderAs(userId);
        await ConfirmAs(userId, orderId);

        var pay = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/pay");
        pay.Headers.Add("x-test-user", userId.ToString());

        var payResponse = await _client.SendAsync(pay);

        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);
    }

    [Fact]
    public async Task POST_pay_without_confirm_returns_400()
    {
        var userId = Guid.NewGuid();

        var orderId = await CreateOrderAs(userId);

        var pay = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/pay");
        pay.Headers.Add("x-test-user", userId.ToString());

        var payResponse = await _client.SendAsync(pay);

        Assert.Equal(HttpStatusCode.BadRequest, payResponse.StatusCode);
    }

    [Fact]
    public async Task POST_pay_as_other_user_returns_403()
    {
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        var orderId = await CreateOrderAs(ownerId);
        await ConfirmAs(ownerId, orderId);

        var pay = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/pay");
        pay.Headers.Add("x-test-user", otherId.ToString());

        var payResponse = await _client.SendAsync(pay);

        Assert.Equal(HttpStatusCode.Forbidden, payResponse.StatusCode);
    }

    [Fact]
    public async Task POST_pay_non_existing_order_returns_404()
    {
        var userId = Guid.NewGuid();

        var pay = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{Guid.NewGuid()}/pay");
        pay.Headers.Add("x-test-user", userId.ToString());

        var payResponse = await _client.SendAsync(pay);

        Assert.Equal(HttpStatusCode.NotFound, payResponse.StatusCode);
    }

    [Fact]
    public async Task POST_pay_without_auth_returns_401()
    {
        // No x-test-user header -> TestAuthHandler fails -> 401
        var payResponse = await _client.PostAsync($"/api/orders/{Guid.NewGuid()}/pay", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, payResponse.StatusCode);
    }

    [Fact]
    public async Task POST_cancel_as_owner_returns_204()
    {
        var userId = Guid.NewGuid();

        var orderId = await CreateOrderAs(userId);

        var cancel = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/cancel");
        cancel.Headers.Add("x-test-user", userId.ToString());

        var cancelResponse = await _client.SendAsync(cancel);

        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task POST_cancel_as_other_user_returns_403()
    {
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        var orderId = await CreateOrderAs(ownerId);

        var cancel = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/cancel");
        cancel.Headers.Add("x-test-user", otherId.ToString());

        var cancelResponse = await _client.SendAsync(cancel);

        Assert.Equal(HttpStatusCode.Forbidden, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task POST_cancel_non_existing_order_returns_404()
    {
        var userId = Guid.NewGuid();

        var cancel = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{Guid.NewGuid()}/cancel");
        cancel.Headers.Add("x-test-user", userId.ToString());

        var cancelResponse = await _client.SendAsync(cancel);

        Assert.Equal(HttpStatusCode.NotFound, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task POST_cancel_without_auth_returns_401()
    {
        var cancelResponse = await _client.PostAsync($"/api/orders/{Guid.NewGuid()}/cancel", content: null);

        Assert.Equal(HttpStatusCode.Unauthorized, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task POST_cancel_twice_is_idempotent_and_returns_204()
    {
        var userId = Guid.NewGuid();

        var orderId = await CreateOrderAs(userId);

        // cancel #1
        var cancel1 = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/cancel");
        cancel1.Headers.Add("x-test-user", userId.ToString());
        var r1 = await _client.SendAsync(cancel1);
        Assert.Equal(HttpStatusCode.NoContent, r1.StatusCode);

        // cancel #2
        var cancel2 = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/cancel");
        cancel2.Headers.Add("x-test-user", userId.ToString());
        var r2 = await _client.SendAsync(cancel2);
        Assert.Equal(HttpStatusCode.NoContent, r2.StatusCode);
    }

    // ---------- Helpers ----------

    private async Task<Guid> CreateOrderAs(Guid userId)
    {
        var request = new
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

        using var create = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
        {
            Content = JsonContent.Create(request)
        };
        create.Headers.Add("x-test-user", userId.ToString());

        var createResponse = await _client.SendAsync(create);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var body = await createResponse.Content.ReadFromJsonAsync<CreateOrderResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.OrderId);

        return body.OrderId;
    }

    private async Task ConfirmAs(Guid userId, Guid orderId)
    {
        using var confirm = new HttpRequestMessage(HttpMethod.Post, $"/api/orders/{orderId}/confirm");
        confirm.Headers.Add("x-test-user", userId.ToString());

        var confirmResponse = await _client.SendAsync(confirm);
        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
    }
    private sealed record CreateOrderResponse(Guid OrderId);
}