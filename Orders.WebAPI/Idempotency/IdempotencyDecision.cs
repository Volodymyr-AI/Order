namespace Orders.WebAPI.Idempotency;

public record IdempotencyDecision(
    bool ShouldExecute,
    Guid RecordId,
    bool Conflict,
    bool InProgress,
    int? CachedStatusCode,
    string? CachedBody)
{
    public static IdempotencyDecision Execute(Guid id) => new(true, id, false, false, null, null);
    public static IdempotencyDecision Cached(Guid id, int code, string body) => new(false, id, false, false, code, body);
    public static IdempotencyDecision ConflictPayload(Guid id) => new(false, id, true, false, null, null);
    public static IdempotencyDecision InProgressDecision(Guid id) => new(false, id, false, true, null, null);
}