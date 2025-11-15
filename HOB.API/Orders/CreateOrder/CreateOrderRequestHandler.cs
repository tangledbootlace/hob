using HOB.Data;
using HOB.Data.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Orders.CreateOrder;

public class CreateOrderRequestHandler : IRequestHandler<CreateOrderRequest, CreateOrderResponse>
{
    private readonly HobDbContext _dbContext;
    private readonly ILogger<CreateOrderRequestHandler> _logger;

    public CreateOrderRequestHandler(HobDbContext dbContext, ILogger<CreateOrderRequestHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        // Validate customer exists
        var customerExists = await _dbContext.Customers
            .AnyAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (!customerExists)
        {
            throw new KeyNotFoundException($"Customer with ID '{request.CustomerId}' not found");
        }

        // Validate sale items
        if (request.SaleItems == null || !request.SaleItems.Any())
        {
            throw new InvalidOperationException("Order must have at least one sale item");
        }

        // Calculate total amount
        var totalAmount = request.SaleItems.Sum(item => item.Quantity * item.UnitPrice);

        // Create order
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            OrderDate = request.OrderDate,
            TotalAmount = totalAmount,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);

        // Create sales
        var sales = new List<Sale>();
        foreach (var saleItem in request.SaleItems)
        {
            var sale = new Sale
            {
                SaleId = Guid.NewGuid(),
                OrderId = order.OrderId,
                ProductName = saleItem.ProductName,
                Quantity = saleItem.Quantity,
                UnitPrice = saleItem.UnitPrice,
                TotalPrice = saleItem.Quantity * saleItem.UnitPrice,
                CreatedAt = DateTime.UtcNow
            };
            sales.Add(sale);
            _dbContext.Sales.Add(sale);
        }

        // Save everything in one transaction
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created order {OrderId} for customer {CustomerId} with {SaleCount} sale items",
            order.OrderId, order.CustomerId, sales.Count);

        return new CreateOrderResponse(
            order.OrderId,
            order.CustomerId,
            order.OrderDate,
            order.TotalAmount,
            order.Status,
            order.CreatedAt,
            order.UpdatedAt,
            sales.Select(s => new SaleItemResponse(
                s.SaleId,
                s.ProductName,
                s.Quantity,
                s.UnitPrice,
                s.TotalPrice
            )).ToList()
        );
    }
}
