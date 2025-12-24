namespace Order.Core.BaseModels;

public sealed class OrderItem
{
    public Guid ProductId { get; }
    public string NameSnapshot { get; }
    public Money UnitPriceSnapshot { get; }
    public int Quantity { get; }
    
    public Money LineTotal => UnitPriceSnapshot * Quantity;

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