namespace HOB.API.Products.ListProducts;

public record ListProductsResponse(
    List<ProductItem> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);

public record ProductItem(
    Guid ProductId,
    string SKU,
    string Name,
    string? Description,
    decimal UnitPrice,
    int StockQuantity,
    int LowStockThreshold,
    string? Category,
    bool IsActive,
    bool IsLowStock,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
