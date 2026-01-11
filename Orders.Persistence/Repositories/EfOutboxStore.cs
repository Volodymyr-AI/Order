using Order.Core.Outbox;

namespace Orders.Persistence.Repositories;

public sealed class EfOutboxStore : IOutboxStore
{
    private readonly OrdersDbContext _db;
    public EfOutboxStore(OrdersDbContext db) => _db = db;
    
    public void Add(OutboxMessage message) => _db.OutboxMessages.Add(message);

    public IReadOnlyList<OutboxMessage> GetUnprocessed(int take = 100)
        => _db.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.OccurredAt)
            .Take(take)
            .ToList();
    
    public OutboxMessage? Find(Guid id) 
        => _db.OutboxMessages.FirstOrDefault(m => m.Id == id);

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

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
       return await _db.SaveChangesAsync(ct);
    }
}