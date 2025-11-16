namespace HOB.API.Products.CreateProduct;

public record CreateProductResponse(
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
    DateTime UpdatedAt
);
