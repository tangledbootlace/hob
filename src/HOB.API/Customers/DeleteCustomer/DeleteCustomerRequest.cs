using MediatR;

namespace HOB.API.Customers.DeleteCustomer;

public record DeleteCustomerRequest(Guid CustomerId) : IRequest;
