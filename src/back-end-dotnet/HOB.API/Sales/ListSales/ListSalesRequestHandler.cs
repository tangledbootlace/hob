using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Sales.ListSales;

public class ListSalesRequestHandler : IRequestHandler<ListSalesRequest, ListSalesResponse>
{
    private readonly HobDbContext _dbContext;

    public ListSalesRequestHandler(HobDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListSalesResponse> Handle(ListSalesRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Sales.AsQueryable();

        // Filter by OrderId if provided
        if (request.OrderId.HasValue)
        {
            query = query.Where(s => s.OrderId == request.OrderId.Value);
        }

        // Filter by ProductName search if provided
        if (!string.IsNullOrWhiteSpace(request.ProductNameSearch))
        {
            query = query.Where(s => s.ProductName.Contains(request.ProductNameSearch));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var sales = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SaleItem(
                s.SaleId,
                s.OrderId,
                s.ProductId,
                s.ProductName,
                s.Quantity,
                s.UnitPrice,
                s.TotalPrice,
                s.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new ListSalesResponse(
            sales,
            request.Page,
            request.PageSize,
            totalItems,
            totalPages
        );
    }
}
