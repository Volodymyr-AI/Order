using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Update;
using Order.Core.DomainEvents;
using Order.Core.Outbox;

namespace Orders.Persistence;

public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    private sealed class PendingState
    {
        public required List<IHasDomainEvents> Aggregates { get; init; }
        public required List<OutboxMessage> Messages { get; init; }
    }

    private readonly ConditionalWeakTable<DbContext, PendingState> _pending = new();

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CollectOutbox(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CollectOutbox(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        FinalizeAfterSuccess(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default
    )
    {
        FinalizeAfterSuccess(eventData.Context);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        RollbackAfterFailure(eventData.Context);
        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        RollbackAfterFailure(eventData.Context);
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void CollectOutbox(DbContext? context)
    {
        if (context == null)
            return;
        
        if(_pending.TryGetValue(context, out _))
            return;
        
        var aggregates = context.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IHasDomainEvents)
            .Select(e => (IHasDomainEvents)e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        if (aggregates.Count == 0)
            return;
        
        var messages = new List<OutboxMessage>(capacity: 16);

        foreach (var agg in aggregates)
        {
            foreach (var ev in agg.DomainEvents)
            {
                if(ev is not DomainEventBase baseEvent)
                    throw new InvalidOperationException("Domain event must inherit DomainEventBase to be stored in Outbox.");
                
                var type = ev.GetType().FullName ?? ev.GetType().Name;
                var payload = System.Text.Json.JsonSerializer.Serialize(ev, ev.GetType());
                
                messages.Add(new OutboxMessage(
                    id: Guid.NewGuid(),
                    occurredAt: baseEvent.OccurredAt,
                    type: type,
                    payloadJson: payload
                ));
            }
        }
        
        context.Set<OutboxMessage>().AddRange(messages);
        
        _pending.Add(context, new PendingState
        {
            Aggregates = aggregates,
            Messages = messages
        });
    }

    private void FinalizeAfterSuccess(DbContext? context)
    {
        if (context == null) return;

        if (_pending.TryGetValue(context, out var state))
        {
            foreach (var agg in state.Aggregates)
            {
                agg.ClearDomainEvents();
            }
            
            _pending.Remove(context);
        }
    }

    private void RollbackAfterFailure(DbContext? context)
    {
        if (context == null) return;
        
        if (_pending.TryGetValue(context, out var state))
        {
            foreach (var msg in state.Messages)
            {
                var entry = context.Entry(msg);
                if(entry.State != EntityState.Detached)
                    entry.State = EntityState.Detached;
            }
            
            _pending.Remove(context);
        }
    }
}