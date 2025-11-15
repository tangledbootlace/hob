using HOB.Data;
using HOB.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Sales.CreateSale;

public class CreateSaleRequestHandler : IRequestHandler<CreateSaleRequest, CreateSaleResponse>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<CreateSaleRequestHandler> _logger;

    public CreateSaleRequestHandler(HobDbContext dbContext, ILogger<CreateSaleRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CreateSaleResponse> Handle(CreateSaleRequest request, CancellationToken cancellationToken)
    {
        // Verify order exists
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID '{request.OrderId}' not found");
        }

        // Calculate total price
        var totalPrice = request.Quantity * request.UnitPrice;

        var sale = new Sale
        {
            SaleId = Guid.NewGuid(),
            OrderId = request.OrderId,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            TotalPrice = totalPrice,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Sales.Add(sale);

        // Update order's total amount
        order.TotalAmount += totalPrice;
        order.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created sale {SaleId} for order {OrderId} with total price {TotalPrice}",
            sale.SaleId, sale.OrderId, sale.TotalPrice);

        return new CreateSaleResponse(
            sale.SaleId,
            sale.OrderId,
            sale.ProductName,
            sale.Quantity,
            sale.UnitPrice,
            sale.TotalPrice,
            sale.CreatedAt
        );
    }
}
