using System.Text.Json;
using Order.Core.DomainEvents;

namespace Order.Core.Outbox;

public static class OutboxCollector
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public static void CollectFromAggregator(IEnumerable<IDomainEvent> domainEvents, IOutboxStore outbox)
    {
        foreach (var ev in domainEvents)
        {
            if(ev is not DomainEventBase baseEvent)
                throw new InvalidOperationException("Domain event must inherit DomainEventBase to be stored in Outbox.");

            var type = ev.GetType().FullName ?? ev.GetType().Name;
            var payload = JsonSerializer.Serialize(ev, ev.GetType(), JsonOptions);

            var msg = new OutboxMessage(
                id: Guid.NewGuid(),
                occurredAt: baseEvent.OccurredAt,
                type: type,
                payloadJson: payload);
            
            outbox.Add(msg);
        }
    }
}