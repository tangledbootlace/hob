namespace HOB.API.Orders.ListOrders;

public record ListOrdersResponse(
    List<OrderItem> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);

public record OrderItem(
    Guid OrderId,
    Guid CustomerId,
    string CustomerName,
    DateTime OrderDate,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
