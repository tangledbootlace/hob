using MediatR;

namespace HOB.API.Sales.GetSale;

public record GetSaleRequest(Guid SaleId) : IRequest<GetSaleResponse>;
