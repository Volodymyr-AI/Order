using Order.Core.Outbox;

namespace Orders.WebAPI.Workers;

public sealed class OutboxDispatcherBackgroundService : BackgroundService
{
    private readonly IOutboxStore _store;
    private readonly IOutboxPublisher _publisher;
    private readonly ILogger<OutboxDispatcherBackgroundService> _log;

    public OutboxDispatcherBackgroundService(
        IOutboxStore store,
        IOutboxPublisher publisher,
        ILogger<OutboxDispatcherBackgroundService> log)
    {
        _store = store;
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
                var batch = _store.GetUnprocessed(take: 100);

                foreach (var msg in batch)
                {
                    try
                    {
                        await _publisher.PublishAsync(msg.Type, msg.PayloadJson, sT);
                        _store.MarkProcessed(msg.Id, DateTimeOffset.UtcNow);
                    }
                    catch (Exception ex)
                    {
                        _store.MarkFailed(msg.Id, ex.Message);
                        _log.LogWarning(ex, "Failed to publish outbox message {Id}", msg.Id);
                    }
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