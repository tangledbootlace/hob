namespace HOB.API.Sales.CreateSale;

public record CreateSaleResponse(
    Guid SaleId,
    Guid OrderId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    DateTime CreatedAt
);
