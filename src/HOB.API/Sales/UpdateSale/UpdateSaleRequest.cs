using MediatR;

namespace HOB.API.Sales.UpdateSale;

public record UpdateSaleRequest(
    Guid SaleId,
    int Quantity,
    decimal UnitPrice
) : IRequest<UpdateSaleResponse>;
