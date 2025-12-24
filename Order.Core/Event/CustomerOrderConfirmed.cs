using Order.Core.BaseModels;

namespace Order.Core.Event;

public sealed record CustomerOrderConfirmed(Guid OrderId, Guid CustomerId, Money Total) 
    : DomainEventBase;