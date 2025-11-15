using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Customers.ListCustomers;

public class ListCustomersRequestHandler : IRequestHandler<ListCustomersRequest, ListCustomersResponse>
{
    private readonly HobDbContext _dbContext;

    public ListCustomersRequestHandler(HobDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ListCustomersResponse> Handle(ListCustomersRequest request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c =>
                c.Name.Contains(request.Search) ||
                c.Email.Contains(request.Search));
        }

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var customers = await query
            .OrderBy(c => c.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerItem(
                c.CustomerId,
                c.Name,
                c.Email,
                c.Phone,
                c.CreatedAt,
                c.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return new ListCustomersResponse(
            customers,
            request.Page,
            request.PageSize,
            totalItems,
            totalPages
        );
    }
}
