using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Products.DeleteProduct;

public class DeleteProductRequestHandler : IRequestHandler<DeleteProductRequest>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<DeleteProductRequestHandler> _logger;

    public DeleteProductRequestHandler(HobDbContext dbContext, ILogger<DeleteProductRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(DeleteProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .Include(p => p.Sales)
            .FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID '{request.ProductId}' not found");
        }

        if (product.Sales.Any())
        {
            throw new InvalidOperationException($"Cannot delete product with existing sales. Product has {product.Sales.Count} sale(s). Consider marking it as inactive instead.");
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted product {ProductId}", request.ProductId);
    }
}
