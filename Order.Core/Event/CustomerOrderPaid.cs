using Order.Core.BaseModels;

namespace Order.Core.Event;

public sealed record CustomerOrderPaid(Guid OrderId, Guid CustomerId, Money Total) 
    : DomainEventBase;