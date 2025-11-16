using HOB.Data;
using HOB.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Products.CreateProduct;

public class CreateProductRequestHandler : IRequestHandler<CreateProductRequest, CreateProductResponse>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<CreateProductRequestHandler> _logger;

    public CreateProductRequestHandler(HobDbContext dbContext, ILogger<CreateProductRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CreateProductResponse> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        // Check if SKU already exists
        var skuExists = await _dbContext.Products
            .AnyAsync(p => p.SKU == request.SKU, cancellationToken);

        if (skuExists)
        {
            throw new InvalidOperationException($"Product with SKU '{request.SKU}' already exists");
        }

        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            SKU = request.SKU,
            Name = request.Name,
            Description = request.Description,
            UnitPrice = request.UnitPrice,
            StockQuantity = request.StockQuantity,
            LowStockThreshold = request.LowStockThreshold,
            Category = request.Category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created product {ProductId} with SKU {SKU}", product.ProductId, product.SKU);

        return new CreateProductResponse(
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
