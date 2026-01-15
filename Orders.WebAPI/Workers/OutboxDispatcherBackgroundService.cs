using Order.Application.Interfaces;
using Order.Core.Outbox;

namespace Orders.WebAPI.Workers;

public sealed class OutboxDispatcherBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOutboxPublisher _publisher;
    private readonly ILogger<OutboxDispatcherBackgroundService> _log;

    public OutboxDispatcherBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOutboxPublisher publisher,
        ILogger<OutboxDispatcherBackgroundService> log)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken sT)
    {
        _log.LogInformation("Outbox dispatcher started");

        while (!sT.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<IOutboxStore>();
                
                var batch = store.GetUnprocessed(take: 100);

                if (batch.Count != 0)
                {
                    foreach (var msg in batch)
                    {
                        try
                        {
                            await _publisher.PublishAsync(msg.Id, msg.Type, msg.PayloadJson, sT);
                            store.MarkProcessed(msg.Id, DateTimeOffset.UtcNow);
                        }
                        catch (Exception ex)
                        {
                            store.MarkFailed(msg.Id, ex.Message);
                            _log.LogWarning(ex, "Failed to publish outbox message {Id}", msg.Id);
                        }
                    }
                    await store.SaveChangesAsync(sT);
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Outbox dispatcher loop failed");
            }
            await Task.Delay(TimeSpan.FromSeconds(2), sT);
        }
        _log.LogInformation("Outbox dispatcher stopped");
    }
}