using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Order.Application.Orders.Queries.GetOrder;

public sealed class CachedGetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDetailsDto?>
{
    private readonly IRequestHandler<GetOrderQuery, OrderDetailsDto?> _inner;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedGetOrderQueryHandler> _logger;

    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    public CachedGetOrderQueryHandler(
        IRequestHandler<GetOrderQuery, OrderDetailsDto?> inner,
        IDistributedCache cache,
        ILogger<CachedGetOrderQueryHandler> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<OrderDetailsDto?> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var cacheKey = $"order:{request.OrderId}";

        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (cached is not null)
        {
            _logger.LogInformation("Cache hit for order {OrderId}", request.OrderId);
            return JsonSerializer.Deserialize<OrderDetailsDto>(cached);
        }

        var result = await _inner.Handle(request, ct);

        if (result is not null)
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(result),
                CacheOptions,
                ct);

            _logger.LogInformation("Cached order {OrderId}", request.OrderId);
        }

        return result;
    }
}