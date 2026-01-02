using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace xUnitTesting.PresentationTests;

public sealed class OrdersEndpointsValidationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    
    public OrdersEndpointsValidationTests(CustomWebApplicationFactory factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task POST_orders_returns_400_with_validation_problem_details_when_items_empty()
    {
        var request = new
        {
            customerId = Guid.NewGuid(),
            storeId = 1,
            items = Array.Empty<object>()
        };
        
        var response = await _client.PostAsJsonAsync("/api/orders", request);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem!.Status);
        
        Assert.True(problem.Errors.Count > 0);
    }
}