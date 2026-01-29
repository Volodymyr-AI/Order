using Microsoft.Extensions.Primitives;
using Order.Application.Interfaces;
using Orders.WebAPI.Middlewares;

namespace Orders.WebAPI.Additional;

public sealed class HttpCorrelationIdAccessor : ICorrelationIdAccessor
{
    private readonly IHttpContextAccessor _http;
    
    public HttpCorrelationIdAccessor(IHttpContextAccessor http) => _http = http;

    public string Get()
    {
        var ctx = _http.HttpContext;
        if (ctx is null) return Guid.NewGuid().ToString("N");

        if (ctx.Items.TryGetValue(CorrelationIdMiddleware.Header, out var v) && v is string s && s.Length > 0)
            return s;

        if (ctx.Request.Headers.TryGetValue(CorrelationIdMiddleware.Header, out var hv) &&
            !StringValues.IsNullOrEmpty(hv))
            return hv.ToString();
        
        return Guid.NewGuid().ToString("N");
    }
}