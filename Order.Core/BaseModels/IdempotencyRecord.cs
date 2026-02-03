namespace Order.Core.BaseModels;

public sealed class IdempotencyRecord
{
    public Guid Id { get; set; }

    public string Scope { get; set; } = default!;          // "POST:/orders"
    public string IdentityType { get; set; } = default!;   // "user"|"client"
    public string IdentityId { get; set; } = default!;     // userId or client_id
    public string Key { get; set; } = default!;            // header value
    public string RequestHash { get; set; } = default!;    // sha256

    public IdempotencyStatus Status { get; set; }
    public int? ResponseCode { get; set; }
    public string? ResponseBody { get; set; }              // json as text
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}