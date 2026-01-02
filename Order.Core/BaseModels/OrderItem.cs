namespace Order.Core.BaseModels;

public sealed class OrderItem
{
    public Guid ProductId { get; private set; }
    public string NameSnapshot { get; private set; } = default!;
    public Money UnitPriceSnapshot { get; private set; } = default!;
    public int Quantity { get; private set; }
    
    public Money LineTotal => UnitPriceSnapshot * Quantity;
    
    private OrderItem() { } // EfCore

    public OrderItem(Guid productId, string nameSnapshot, Money unitPriceSnapshot, int quantity)
    {
        if(productId == Guid.Empty) throw new ArgumentException("ProductId is required.", nameof(productId));
        if(string.IsNullOrWhiteSpace(nameSnapshot)) throw new ArgumentException("NameSnapshot is required.", nameof(nameSnapshot));
        if(quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be > 0.");
        
        ProductId = productId;
        NameSnapshot = nameSnapshot;
        UnitPriceSnapshot = unitPriceSnapshot;
        Quantity = quantity;
    }
}