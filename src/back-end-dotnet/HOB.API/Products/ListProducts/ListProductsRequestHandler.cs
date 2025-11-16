using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Products.ListProducts;

public class ListProductsRequestHandler : IRequestHandler<ListProductsRequest, ListProductsResponse>
{
    private readonly HobDbContext _dbContext;

    public ListProductsRequestHandler(HobDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListProductsResponse> Handle(ListProductsRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Products.AsQueryable();

        // Filter by active status
        if (request.ActiveOnly.HasValue && request.ActiveOnly.Value)
        {
            query = query.Where(p => p.IsActive);
        }

        // Search by name, SKU, or description
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(p =>
                p.Name.Contains(request.Search) ||
                p.SKU.Contains(request.Search) ||
                (p.Description != null && p.Description.Contains(request.Search)));
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(p => p.Category == request.Category);
        }

        // Filter by low stock
        if (request.LowStock.HasValue && request.LowStock.Value)
        {
            query = query.Where(p => p.StockQuantity <= p.LowStockThreshold);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductItem(
                p.ProductId,
                p.SKU,
                p.Name,
                p.Description,
                p.UnitPrice,
                p.StockQuantity,
                p.LowStockThreshold,
                p.Category,
                p.IsActive,
                p.StockQuantity <= p.LowStockThreshold,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new ListProductsResponse(
            products,
            request.Page,
            request.PageSize,
            totalItems,
            totalPages
        );
    }
}
