namespace Order.Core.Outbox;

public class InMemoryOutboxStore : IOutboxStore
{
    private readonly List<OutboxMessage> _messages = new();
    
    public void Add(OutboxMessage message) => _messages.Add(message);
    
    public IReadOnlyList<OutboxMessage> GetUnprocessed(int take = 100) 
            => _messages.Where(m => m.ProcessedAt == null).Take(take).ToList();

    public OutboxMessage? Find(Guid id) => _messages.FirstOrDefault(m => m.Id == id);

    public void MarkProcessed(Guid id, DateTimeOffset processedAt)
    {
        var msg = Find(id);
        if (msg is null) return;
        msg.MarkProcessed(processedAt);
    }

    public void MarkFailed(Guid id, string error)
    {
        var msg = Find(id);
        if (msg is null) return;
        msg.MarkFailed(error);
    }
}