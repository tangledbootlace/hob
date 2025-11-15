using MediatR;

namespace HOB.API.Customers.ListCustomers;

public record ListCustomersRequest(
    int Page = 1,
    int PageSize = 20,
    string? Search = null
) : IRequest<ListCustomersResponse>;
