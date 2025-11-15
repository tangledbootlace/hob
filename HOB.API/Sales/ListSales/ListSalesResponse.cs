namespace HOB.API.Sales.ListSales;

public record ListSalesResponse(
    List<SaleItem> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);

public record SaleItem(
    Guid SaleId,
    Guid OrderId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    DateTime CreatedAt
);
