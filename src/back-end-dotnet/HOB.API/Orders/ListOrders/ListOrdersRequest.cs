using MediatR;

namespace HOB.API.Orders.ListOrders;

public record ListOrdersRequest(
    int Page = 1,
    int PageSize = 20,
    Guid? CustomerId = null,
    string? Status = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<ListOrdersResponse>;
