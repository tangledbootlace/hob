namespace HOB.API.Sales.GetSale;

public record GetSaleResponse(
    Guid SaleId,
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    DateTime CreatedAt,
    OrderDetails Order
);

public record OrderDetails(
    Guid OrderId,
    Guid CustomerId,
    DateTime OrderDate,
    decimal TotalAmount,
    string Status
);
