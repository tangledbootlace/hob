using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Orders.ListOrders;

public class ListOrdersRequestHandler : IRequestHandler<ListOrdersRequest, ListOrdersResponse>
{
    private readonly HobDbContext _dbContext;

    public ListOrdersRequestHandler(HobDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListOrdersResponse> Handle(ListOrdersRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Orders
            .Include(o => o.Customer)
            .AsQueryable();

        // Apply filters
        if (request.CustomerId.HasValue)
        {
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(o => o.Status == request.Status);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= request.EndDate.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderItem(
                o.OrderId,
                o.CustomerId,
                o.Customer.Name,
                o.OrderDate,
                o.TotalAmount,
                o.Status,
                o.CreatedAt,
                o.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new ListOrdersResponse(
            orders,
            request.Page,
            request.PageSize,
            totalItems,
            totalPages
        );
    }
}
