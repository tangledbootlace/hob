using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Customers.UpdateCustomer;

public class UpdateCustomerRequestHandler : IRequestHandler<UpdateCustomerRequest, UpdateCustomerResponse>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<UpdateCustomerRequestHandler> _logger;

    public UpdateCustomerRequestHandler(HobDbContext dbContext, ILogger<UpdateCustomerRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UpdateCustomerResponse> Handle(UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await _dbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID '{request.CustomerId}' not found");
        }

        // Check if email is being changed to one that already exists
        if (customer.Email != request.Email)
        {
            var emailExists = await _dbContext.Customers
                .AnyAsync(c => c.Email == request.Email && c.CustomerId != request.CustomerId, cancellationToken);

            if (emailExists)
            {
                throw new InvalidOperationException($"Customer with email '{request.Email}' already exists");
            }
        }

        customer.Name = request.Name;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated customer {CustomerId}", customer.CustomerId);

        return new UpdateCustomerResponse(
            customer.CustomerId,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.CreatedAt,
            customer.UpdatedAt
        );
    }
}
