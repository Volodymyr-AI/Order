using Order.Core.Event;

namespace Order.Core.BaseModels;

public class CustomerOrder
{
    private readonly List<OrderItem> _items = new(); 
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public int StoreId { get; private set; }

    private Currency? _currency;
    public Currency Currency => _currency ?? throw new InvalidOperationException("Order currency is not set.");
    // Potential problem ( previously removed -> = Money.Zero(Currency.FromCode("USD")); for default )) Watch through tests and behaviour
    public Money Total { get; private set; } 
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;
    
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    
    
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    private void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    
    private CustomerOrder() {}

    public CustomerOrder(Guid id, Guid customerId, int storeId)
    {
        if(id == Guid.Empty) throw new ArgumentException("Id is required.", nameof(id));
        if(customerId == Guid.Empty) throw new ArgumentException("CustomerId is required.", nameof(customerId));
        if(storeId <= 0) throw new ArgumentOutOfRangeException(nameof(storeId));

        Id = id;
        CustomerId = customerId;
        StoreId = storeId;
        Status = OrderStatus.Draft;
    }

    public void AddItem(OrderItem item)
    {
        if(item is null) throw new ArgumentNullException(nameof(item));
        EnsureDraft();

        if (_items.Count == 0)
        {
            _currency = item.UnitPriceSnapshot.Currency;
            Total = Money.Zero(_currency.Value);
        }
        else
        {
            Money.EnsureSameCurrency(Total, item.UnitPriceSnapshot);
        }
        
        _items.Add(item);
        RecalculateTotal();
    }

    public void RemoveItem(Guid productId)
    {
        EnsureDraft();
        
        if(productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.", nameof(productId));
        
        var index = _items.FindIndex(x => x.ProductId == productId);
        if(index < 0)
            throw new InvalidOperationException("Item not found.");
        
        _items.RemoveAt(index);
        RecalculateTotal();
    }

    public void ChangeQuantity(Guid productId, int newQuantity)
    {
        EnsureDraft();
        if(productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.", nameof(productId));
        if(newQuantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(newQuantity), "Quantity must be > 0.");
        
        var index = _items.FindIndex(x => x.ProductId == productId);
        if(index < 0)
            throw new InvalidOperationException("Item not found.");
        
        var old =  _items[index];
        var updated = new OrderItem(old.ProductId, old.NameSnapshot, old.UnitPriceSnapshot, newQuantity);

        _items[index] = updated;
        RecalculateTotal();
    }

    public void Confirm()
    {
        EnsureDraft();
        
        if(_items.Count == 0)
            throw new InvalidOperationException("Cannot confirm an empty order.");
        
        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
        
        AddDomainEvent(new CustomerOrderConfirmed(Id, CustomerId, Total));
    }

    public void Pay()
    {
        if(Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("CustomerOrder must be confirmed before payment.");

        Status = OrderStatus.Paid;
        PaidAt = DateTimeOffset.UtcNow;
        
        AddDomainEvent(new CustomerOrderPaid(Id, CustomerId, Total));  
    }

    public void Cancel()
    {
        if(Status == OrderStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a paid order.");

        if (Status == OrderStatus.Cancelled)
            return;
        
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
    }
    
    public void ClearDomainEvents() => _domainEvents.Clear();

    private void RecalculateTotal()
    {
        if (_items.Count == 0)
        {
            if(_currency is null)
                throw new InvalidOperationException("Order currency is not set.");
            Total = Money.Zero(_currency.Value);
            return;
        }

        var sum = Money.Zero(_items[0].UnitPriceSnapshot.Currency);
        foreach (var item in _items)
        {
            sum += item.LineTotal;
        }
        
        Total = sum;
    }

    private void EnsureDraft()
    {
        if(Status != OrderStatus.Draft)
            throw new InvalidOperationException("CustomerOrder items can be changed only in Draft status.");
    }
}