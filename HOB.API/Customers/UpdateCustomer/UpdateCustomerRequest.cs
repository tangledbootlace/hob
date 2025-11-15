using MediatR;

namespace HOB.API.Customers.UpdateCustomer;

public record UpdateCustomerRequest(
    Guid CustomerId,
    string Name,
    string Email,
    string? Phone
) : IRequest<UpdateCustomerResponse>;
