using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Products.GetProduct;

public class GetProductRequestHandler : IRequestHandler<GetProductRequest, GetProductResponse>
{
    private readonly HobDbContext _dbContext;

    public GetProductRequestHandler(HobDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetProductResponse> Handle(GetProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID '{request.ProductId}' not found");
        }

        return new GetProductResponse(
            product.ProductId,
            product.SKU,
            product.Name,
            product.Description,
            product.UnitPrice,
            product.StockQuantity,
            product.LowStockThreshold,
            product.Category,
            product.IsActive,
            product.CreatedAt,
            product.UpdatedAt
        );
    }
}
