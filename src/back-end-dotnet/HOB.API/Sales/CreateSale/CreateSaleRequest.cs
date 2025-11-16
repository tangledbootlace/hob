using MediatR;

namespace HOB.API.Sales.CreateSale;

public record CreateSaleRequest(
    Guid OrderId,
    Guid ProductId,
    int Quantity
) : IRequest<CreateSaleResponse>;
