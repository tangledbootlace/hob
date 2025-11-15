using MediatR;

namespace HOB.API.Sales.DeleteSale;

public record DeleteSaleRequest(Guid SaleId) : IRequest;
