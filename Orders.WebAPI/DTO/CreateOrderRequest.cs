namespace Orders.WebAPI.DTO;

public sealed record CreateOrderRequest(
    Guid CustomerId,
    int StoreId,
    List<CreateOrderItemRequest> Items
);