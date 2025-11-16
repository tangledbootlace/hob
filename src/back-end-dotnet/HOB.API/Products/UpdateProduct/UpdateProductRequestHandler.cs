using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Products.UpdateProduct;

public class UpdateProductRequestHandler : IRequestHandler<UpdateProductRequest, UpdateProductResponse>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<UpdateProductRequestHandler> _logger;

    public UpdateProductRequestHandler(HobDbContext dbContext, ILogger<UpdateProductRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UpdateProductResponse> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID '{request.ProductId}' not found");
        }

        // Check if SKU is being changed to one that already exists
        if (product.SKU != request.SKU)
        {
            var skuExists = await _dbContext.Products
                .AnyAsync(p => p.SKU == request.SKU && p.ProductId != request.ProductId, cancellationToken);

            if (skuExists)
            {
                throw new InvalidOperationException($"Product with SKU '{request.SKU}' already exists");
            }
        }

        product.SKU = request.SKU;
        product.Name = request.Name;
        product.Description = request.Description;
        product.UnitPrice = request.UnitPrice;
        product.StockQuantity = request.StockQuantity;
        product.LowStockThreshold = request.LowStockThreshold;
        product.Category = request.Category;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated product {ProductId}", product.ProductId);

        return new UpdateProductResponse(
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
