namespace Order.Core.Outbox;

public interface IOutboxPublisher
{
    Task PublishAsync(Guid messageId, string type, string payloadJson, CancellationToken ct);
}