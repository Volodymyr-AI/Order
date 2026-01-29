using Microsoft.Extensions.Primitives;

namespace Orders.WebAPI.Middlewares;

public sealed class CorrelationIdMiddleware
{
    public const string Header = "X-Correlation-Id";
    private readonly RequestDelegate _next;
    
    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx, ILogger<CorrelationIdMiddleware> log)
    {
        var cid = ctx.Request.Headers.TryGetValue(Header, out var value) && !StringValues.IsNullOrEmpty(value)
            ? value.ToString()
            : Guid.NewGuid().ToString("N");

        ctx.Items[Header] = cid;
        ctx.Response.Headers[Header] = cid;

        using (log.BeginScope(new Dictionary<string, object>
               {
                   ["correlationId"] = cid
               }))
        {
            await _next(ctx);
        }
    }
}