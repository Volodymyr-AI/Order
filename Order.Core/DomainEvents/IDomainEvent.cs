namespace Order.Core.DomainEvents;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}