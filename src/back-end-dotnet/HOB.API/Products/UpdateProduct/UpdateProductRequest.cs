using MediatR;

namespace HOB.API.Products.UpdateProduct;

public record UpdateProductRequest(
    Guid ProductId,
    string SKU,
    string Name,
    string? Description,
    decimal UnitPrice,
    int StockQuantity,
    int LowStockThreshold,
    string? Category,
    bool IsActive
) : IRequest<UpdateProductResponse>;
