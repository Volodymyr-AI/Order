namespace Order.Core.Outbox;

public interface IOutboxStore
{
    void Add(OutboxMessage message);
    IReadOnlyList<OutboxMessage> GetUnprocessed(int take = 100);
    OutboxMessage? Find(Guid id);
    
    void MarkProcessed(Guid id, DateTimeOffset processedAt);
    void MarkFailed(Guid id, string error);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}