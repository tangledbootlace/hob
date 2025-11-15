using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Customers.DeleteCustomer;

public class DeleteCustomerRequestHandler : IRequestHandler<DeleteCustomerRequest>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<DeleteCustomerRequestHandler> _logger;

    public DeleteCustomerRequestHandler(HobDbContext dbContext, ILogger<DeleteCustomerRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(DeleteCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID '{request.CustomerId}' not found");
        }

        if (customer.Orders.Any())
        {
            throw new InvalidOperationException($"Cannot delete customer with existing orders. Customer has {customer.Orders.Count} order(s).");
        }

        _dbContext.Customers.Remove(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted customer {CustomerId}", request.CustomerId);
    }
}
