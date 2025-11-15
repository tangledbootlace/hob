using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Orders.DeleteOrder;

public class DeleteOrderRequestHandler : IRequestHandler<DeleteOrderRequest>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<DeleteOrderRequestHandler> _logger;

    public DeleteOrderRequestHandler(HobDbContext dbContext, ILogger<DeleteOrderRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(DeleteOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .Include(o => o.Sales)
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID '{request.OrderId}' not found");
        }

        // Only allow deleting orders with status "Pending" or "Cancelled"
        if (order.Status != "Pending" && order.Status != "Cancelled")
        {
            throw new InvalidOperationException($"Cannot delete order with status '{order.Status}'. Only orders with status 'Pending' or 'Cancelled' can be deleted.");
        }

        _dbContext.Orders.Remove(order);
        // Sales will be cascade deleted due to the relationship configuration in HobDbContext
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted order {OrderId} with {SaleCount} sale items", request.OrderId, order.Sales.Count);
    }
}
