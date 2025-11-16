namespace HOB.API.Products.UpdateProduct;

public record UpdateProductResponse(
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
