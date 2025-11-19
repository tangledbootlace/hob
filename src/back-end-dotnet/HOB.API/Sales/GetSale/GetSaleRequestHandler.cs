using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Sales.GetSale;

public class GetSaleRequestHandler : IRequestHandler<GetSaleRequest, GetSaleResponse>
{
    private readonly HobDbContext _dbContext;

    public GetSaleRequestHandler(HobDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetSaleResponse> Handle(GetSaleRequest request, CancellationToken cancellationToken)
    {
        var sale = await _dbContext.Sales
            .Include(s => s.Order)
                .ThenInclude(o => o.Customer)
            .Include(s => s.Order)
                .ThenInclude(o => o.Sales)
            .FirstOrDefaultAsync(s => s.SaleId == request.SaleId, cancellationToken);

        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID '{request.SaleId}' not found");
        }

        var orderDetails = new OrderDetails(
            sale.Order.OrderId,
            sale.Order.CustomerId,
            sale.Order.OrderDate,
            sale.Order.TotalAmount,
            sale.Order.Status,
            sale.Order.Customer.Name,
            sale.Order.Sales.Count
        );

        return new GetSaleResponse(
            sale.SaleId,
            sale.OrderId,
            sale.ProductId,
            sale.ProductName,
            sale.Quantity,
            sale.UnitPrice,
            sale.TotalPrice,
            sale.CreatedAt,
            orderDetails
        );
    }
}
