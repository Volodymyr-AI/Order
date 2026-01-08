using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using xUnitTesting.PresentationTests.Additional;

namespace xUnitTesting.PresentationTests;

public sealed class OrdersJwtAuthTests : IClassFixture<JwtWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string JwtKey = "2Fwd7vdJvGunASlbzD+SAE/KteGRuxsrOY4G50++o3M=";

    public OrdersJwtAuthTests(JwtWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GET_order_returns_200_with_valid_bearer_token()
    {
        var userId = Guid.NewGuid();
        var token = CreateJwt(userId, JwtKey);

        var createRequest = new
        {
            customerId = userId,
            storeId = 1,
            items = new[]
            {
                new
                {
                    productId = Guid.NewGuid(), nameSnapshot = "Item A", unitPriceAmount = 10m, currencyCode = "USD",
                    quantity = 1
                }
            }
        };

        var created = await (await _client.PostAsJsonAsync("/api/orders", createRequest))
            .Content.ReadFromJsonAsync<CreateOrderResponse>();

        var req = new HttpRequestMessage(HttpMethod.Get, $"/api/orders/{created!.OrderId}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var resp = await _client.SendAsync(req);
        
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    private sealed record CreateOrderResponse(Guid OrderId);

    private static string CreateJwt(Guid userId, string key)
    {
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) },
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}