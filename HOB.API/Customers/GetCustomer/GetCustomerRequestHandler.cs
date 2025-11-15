using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Customers.GetCustomer;

public class GetCustomerRequestHandler : IRequestHandler<GetCustomerRequest, GetCustomerResponse>
{
    private readonly HobDbContext _dbContext;

    public GetCustomerRequestHandler(HobDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetCustomerResponse> Handle(GetCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID '{request.CustomerId}' not found");
        }

        var orders = customer.Orders
            .Select(o => new OrderSummary(o.OrderId, o.OrderDate, o.TotalAmount, o.Status))
            .ToList();

        return new GetCustomerResponse(
            customer.CustomerId,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.CreatedAt,
            customer.UpdatedAt,
            orders
        );
    }
}
