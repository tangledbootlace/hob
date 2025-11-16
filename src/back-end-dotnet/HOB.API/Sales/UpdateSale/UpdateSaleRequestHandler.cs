using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Sales.UpdateSale;

public class UpdateSaleRequestHandler : IRequestHandler<UpdateSaleRequest, UpdateSaleResponse>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<UpdateSaleRequestHandler> _logger;

    public UpdateSaleRequestHandler(HobDbContext dbContext, ILogger<UpdateSaleRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UpdateSaleResponse> Handle(UpdateSaleRequest request, CancellationToken cancellationToken)
    {
        var sale = await _dbContext.Sales
            .Include(s => s.Order)
            .FirstOrDefaultAsync(s => s.SaleId == request.SaleId, cancellationToken);

        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID '{request.SaleId}' not found");
        }

        // Store old total price for order amount adjustment
        var oldTotalPrice = sale.TotalPrice;

        // Update sale properties
        sale.Quantity = request.Quantity;
        sale.UnitPrice = request.UnitPrice;
        sale.TotalPrice = request.Quantity * request.UnitPrice;

        // Update order's total amount (subtract old, add new)
        sale.Order.TotalAmount = sale.Order.TotalAmount - oldTotalPrice + sale.TotalPrice;
        sale.Order.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated sale {SaleId} with new total price {TotalPrice}",
            sale.SaleId, sale.TotalPrice);

        return new UpdateSaleResponse(
            sale.SaleId,
            sale.OrderId,
            sale.ProductId,
            sale.ProductName,
            sale.Quantity,
            sale.UnitPrice,
            sale.TotalPrice,
            sale.CreatedAt
        );
    }
}
