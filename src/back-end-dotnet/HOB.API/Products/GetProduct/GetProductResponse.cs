namespace HOB.API.Products.GetProduct;

public record GetProductResponse(
    Guid ProductId,
    string SKU,
    string Name,
    string? Description,
    decimal UnitPrice,
    int StockQuantity,
    int LowStockThreshold,
    string? Category,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int SalesCount
);
