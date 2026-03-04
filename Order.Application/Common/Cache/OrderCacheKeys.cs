using Microsoft.Extensions.Caching.Distributed;

namespace Order.Application.Common.Cache;

public static class OrderCacheKeys
{
    public static string Order(Guid orderId) => $"order:{orderId}";
    
    public static Task InvalidateOrderAsync(
        IDistributedCache cache,
        Guid orderId,
        CancellationToken ct = default)
        => cache.RemoveAsync(Order(orderId), ct);
}