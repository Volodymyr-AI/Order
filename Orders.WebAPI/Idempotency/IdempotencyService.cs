using Microsoft.EntityFrameworkCore;
using Order.Application.Interfaces;
using Order.Core.BaseModels;
using Orders.Persistence;

namespace Orders.WebAPI.Idempotency;

public sealed class IdempotencyService
{
    private readonly OrdersDbContext _db;
    
    public IdempotencyService(OrdersDbContext db) => _db = db;

    public async Task<IdempotencyDecision> PreProcessAsync(string scope, RequestIdentity identity, string key,
        string requestHash, CancellationToken ct)
    {
        var record = new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            Scope = scope,
            IdentityType = identity.Type,
            IdentityId = identity.Id,
            Key = key,
            RequestHash = requestHash,
            Status = IdempotencyStatus.InProgress,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        _db.IdempotencyKeys.Add(record);

        try
        {
            await _db.SaveChangesAsync(ct);
            return IdempotencyDecision.Execute(record.Id);
        }
        catch (DbUpdateException)
        {
            _db.ChangeTracker.Clear();

            var existing = await _db.IdempotencyKeys
                .SingleAsync(x => x.Scope == scope
                                  && x.IdentityId == identity.Type
                                  && x.IdentityId == identity.Id
                                  && x.Key == key, ct);
            
            if(!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
                return IdempotencyDecision.ConflictPayload(existing.Id);

            return existing.Status switch
            {
                IdempotencyStatus.Completed => IdempotencyDecision.Cached(
                    existing.Id, existing.ResponseCode ?? 200, existing.ResponseBody ?? ""),
                IdempotencyStatus.InProgress => IdempotencyDecision.InProgressDecision(existing.Id),
                _ => IdempotencyDecision.InProgressDecision(existing.Id)
            };
        }
    }

    public async Task SaveCompletedAsync(Guid id, int statusCode, string responseBody, CancellationToken ct)
    {
        var rec = await _db.IdempotencyKeys.SingleAsync(x => x.Id == id, ct);
        rec.Status = IdempotencyStatus.Completed;
        rec.ResponseCode = statusCode;
        rec.ResponseBody = responseBody;
        rec.CompletedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }
}