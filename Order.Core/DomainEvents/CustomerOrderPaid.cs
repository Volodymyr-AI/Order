using Order.Core.BaseModels;

namespace Order.Core.DomainEvents;

public sealed record CustomerOrderPaid(Guid OrderId, Guid CustomerId, Money Total) 
    : DomainEventBase;