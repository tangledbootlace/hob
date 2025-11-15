using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Orders.GetOrder;

public class GetOrderRequestHandler : IRequestHandler<GetOrderRequest, GetOrderResponse>
{
    private readonly HobDbContext _dbContext;

    public GetOrderRequestHandler(HobDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetOrderResponse> Handle(GetOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.Sales)
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID '{request.OrderId}' not found");
        }

        var customer = new CustomerDetails(
            order.Customer.CustomerId,
            order.Customer.Name,
            order.Customer.Email,
            order.Customer.Phone
        );

        var sales = order.Sales
            .Select(s => new SaleDetails(
                s.SaleId,
                s.ProductName,
                s.Quantity,
                s.UnitPrice,
                s.TotalPrice
            ))
            .ToList();

        return new GetOrderResponse(
            order.OrderId,
            order.CustomerId,
            order.OrderDate,
            order.TotalAmount,
            order.Status,
            order.CreatedAt,
            order.UpdatedAt,
            customer,
            sales
        );
    }
}
