using HOB.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Orders.UpdateOrder;

public class UpdateOrderRequestHandler : IRequestHandler<UpdateOrderRequest, UpdateOrderResponse>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<UpdateOrderRequestHandler> _logger;

    public UpdateOrderRequestHandler(HobDbContext dbContext, ILogger<UpdateOrderRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UpdateOrderResponse> Handle(UpdateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID '{request.OrderId}' not found");
        }

        // Only update status
        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated order {OrderId} status to {Status}", order.OrderId, order.Status);

        return new UpdateOrderResponse(
            order.OrderId,
            order.CustomerId,
            order.OrderDate,
            order.TotalAmount,
            order.Status,
            order.CreatedAt,
            order.UpdatedAt
        );
    }
}
