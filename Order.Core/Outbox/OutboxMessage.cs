namespace Order.Core.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; }
    public DateTimeOffset OccurredAt { get; }
    public string Type { get; }
    public string PayloadJson { get; }
    
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    public OutboxMessage(Guid id, DateTimeOffset occurredAt, string type, string payloadJson)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type is required.", nameof(type));
        if (string.IsNullOrWhiteSpace(payloadJson)) throw new ArgumentException("PayloadJson is required.", nameof(payloadJson));
        
        Id = id;
        OccurredAt = occurredAt;
        Type = type;
        PayloadJson = payloadJson;
    }
    
    public void MarkProcessed(DateTimeOffset processedAt) => ProcessedAt = processedAt;

    public void MarkFailed(string error)
    {
        Attempts++;
        LastError = error;
    }
}