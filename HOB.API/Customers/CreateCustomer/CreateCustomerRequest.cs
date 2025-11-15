using MediatR;

namespace HOB.API.Customers.CreateCustomer;

public record CreateCustomerRequest(
    string Name,
    string Email,
    string? Phone
) : IRequest<CreateCustomerResponse>;
