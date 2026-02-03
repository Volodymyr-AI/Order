namespace Orders.WebAPI.Middlewares;

public sealed class ClientIdMiddleware
{
    public const string CookieName = "client_id";
    private readonly RequestDelegate _next;
    
    public ClientIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Cookies.TryGetValue(CookieName, out var clientId) || string.IsNullOrEmpty(clientId) ||
            clientId.Length > 64)
        {
            clientId = Guid.NewGuid().ToString("N");

            context.Response.Cookies.Append(CookieName, clientId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/"
            });
        }

        context.Items[CookieName] = clientId;
        await _next(context);
    }
}