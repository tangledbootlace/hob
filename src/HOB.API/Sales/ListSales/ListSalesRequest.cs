using MediatR;

namespace HOB.API.Sales.ListSales;

public record ListSalesRequest(
    int Page = 1,
    int PageSize = 20,
    Guid? OrderId = null,
    string? ProductNameSearch = null
) : IRequest<ListSalesResponse>;
