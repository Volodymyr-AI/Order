using Order.Core.BaseModels;

namespace Order.Core.DomainEvents;

public sealed record CustomerOrderConfirmed(Guid OrderId, Guid CustomerId, Money Total) 
    : DomainEventBase;