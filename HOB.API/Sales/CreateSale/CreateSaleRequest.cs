using MediatR;

namespace HOB.API.Sales.CreateSale;

public record CreateSaleRequest(
    Guid OrderId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
) : IRequest<CreateSaleResponse>;
