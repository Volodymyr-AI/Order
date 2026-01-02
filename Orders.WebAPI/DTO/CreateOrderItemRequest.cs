namespace Orders.WebAPI.DTO;

public record CreateOrderItemRequest(
    Guid ProductId,
    string NameSnapshot,
    decimal UnitPriceAmount,
    string CurrencyCode,
    int Quantity);