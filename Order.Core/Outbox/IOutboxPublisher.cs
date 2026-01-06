namespace Order.Core.Outbox;

public interface IOutboxPublisher
{
    Task PublishAsync(string type, string payloadJson, CancellationToken ct);
}