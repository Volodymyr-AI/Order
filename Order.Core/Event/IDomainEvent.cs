namespace Order.Core.Event;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}