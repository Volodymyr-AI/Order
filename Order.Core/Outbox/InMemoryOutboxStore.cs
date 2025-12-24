namespace Order.Core.Outbox;

public class InMemoryOutboxStore : IOutboxStore
{
    private readonly List<OutboxMessage> _messages = new();
    
    public void Add(OutboxMessage message) => _messages.Add(message);
    
    public IReadOnlyList<OutboxMessage> GetUnprocessed(int take = 100) 
            => _messages.Where(m => m.ProcessedAt == null).Take(take).ToList();

    public OutboxMessage? Find(Guid id) => _messages.FirstOrDefault(m => m.Id == id);
}