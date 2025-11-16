namespace HOB.API.Sales.UpdateSale;

public record UpdateSaleResponse(
    Guid SaleId,
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    DateTime CreatedAt
);
