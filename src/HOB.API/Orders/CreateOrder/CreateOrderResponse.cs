namespace HOB.API.Orders.CreateOrder;

public record CreateOrderResponse(
    Guid OrderId,
    Guid CustomerId,
    DateTime OrderDate,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<SaleItemResponse> Sales
);

public record SaleItemResponse(
    Guid SaleId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
