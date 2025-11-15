using HOB.Data;
using HOB.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Customers.CreateCustomer;

public class CreateCustomerRequestHandler : IRequestHandler<CreateCustomerRequest, CreateCustomerResponse>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<CreateCustomerRequestHandler> _logger;

    public CreateCustomerRequestHandler(HobDbContext dbContext, ILogger<CreateCustomerRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CreateCustomerResponse> Handle(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var emailExists = await _dbContext.Customers
            .AnyAsync(c => c.Email == request.Email, cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException($"Customer with email '{request.Email}' already exists");
        }

        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created customer {CustomerId} with email {Email}", customer.CustomerId, customer.Email);

        return new CreateCustomerResponse(
            customer.CustomerId,
            customer.Name,
            customer.Email,
            customer.Phone,
            customer.CreatedAt,
            customer.UpdatedAt
        );
    }
}
