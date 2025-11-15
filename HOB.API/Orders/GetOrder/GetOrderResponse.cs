namespace HOB.API.Orders.GetOrder;

public record GetOrderResponse(
    Guid OrderId,
    Guid CustomerId,
    DateTime OrderDate,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    CustomerDetails Customer,
    List<SaleDetails> Sales
);

public record CustomerDetails(
    Guid CustomerId,
    string Name,
    string Email,
    string? Phone
);

public record SaleDetails(
    Guid SaleId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
