using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Sales.DeleteSale;

public class DeleteSaleRequestHandler : IRequestHandler<DeleteSaleRequest>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<DeleteSaleRequestHandler> _logger;

    public DeleteSaleRequestHandler(HobDbContext dbContext, ILogger<DeleteSaleRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(DeleteSaleRequest request, CancellationToken cancellationToken)
    {
        var sale = await _dbContext.Sales
            .Include(s => s.Order)
                .ThenInclude(o => o.Sales)
            .FirstOrDefaultAsync(s => s.SaleId == request.SaleId, cancellationToken);

        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID '{request.SaleId}' not found");
        }

        // Check if order status is Pending
        if (sale.Order.Status != "Pending")
        {
            throw new InvalidOperationException($"Cannot delete sale from order with status '{sale.Order.Status}'. Only sales from orders with status 'Pending' can be deleted.");
        }

        // Check if this is the last sale in the order
        if (sale.Order.Sales.Count <= 1)
        {
            throw new InvalidOperationException("Cannot delete the last sale in an order. An order must have at least one sale.");
        }

        // Update order's total amount
        sale.Order.TotalAmount -= sale.TotalPrice;
        sale.Order.UpdatedAt = DateTime.UtcNow;

        _dbContext.Sales.Remove(sale);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted sale {SaleId} from order {OrderId}", request.SaleId, sale.OrderId);
    }
}
