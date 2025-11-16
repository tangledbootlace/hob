using MediatR;

namespace HOB.API.Products.CreateProduct;

public record CreateProductRequest(
    string SKU,
    string Name,
    string? Description,
    decimal UnitPrice,
    int StockQuantity,
    int LowStockThreshold,
    string? Category
) : IRequest<CreateProductResponse>;
