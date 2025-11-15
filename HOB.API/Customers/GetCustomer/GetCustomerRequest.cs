using MediatR;

namespace HOB.API.Customers.GetCustomer;

public record GetCustomerRequest(Guid CustomerId) : IRequest<GetCustomerResponse>;
