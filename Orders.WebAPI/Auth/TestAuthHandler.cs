using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Orders.WebAPI.Auth;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "Test";
    
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers.TryGetValue("x-test-user", out var v)
            ? v.ToString()
               : null;

        if (!Guid.TryParse(userId, out var id))
            return Task.FromResult(AuthenticateResult.Fail("Missing or invalid x-test-user header"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);
        
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}