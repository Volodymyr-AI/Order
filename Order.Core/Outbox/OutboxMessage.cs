namespace Order.Core.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string Type { get; private set; } = default!;
    public string PayloadJson { get; private set; } = default!;
    public string CorrelationId { get; private set; } = default!;
    
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }
    
    private OutboxMessage() { }

    public OutboxMessage(Guid id, DateTimeOffset occurredAt, string type, string payloadJson, string correlationId)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type is required.", nameof(type));
        if (string.IsNullOrWhiteSpace(payloadJson)) throw new ArgumentException("PayloadJson is required.", nameof(payloadJson));
        if(string.IsNullOrWhiteSpace(correlationId)) throw new ArgumentException("CorrelationId is required.", nameof(correlationId));
        
        Id = id;
        OccurredAt = occurredAt;
        Type = type;
        PayloadJson = payloadJson;
        CorrelationId = correlationId;
    }
    
    public void MarkProcessed(DateTimeOffset processedAt) => ProcessedAt = processedAt;

    public void MarkFailed(string error)
    {
        Attempts++;
        LastError = error;
    }
}