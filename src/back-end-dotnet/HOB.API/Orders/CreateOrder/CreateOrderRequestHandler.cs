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

        // Get all product IDs from sale items
        var productIds = request.SaleItems.Select(si => si.ProductId).Distinct().ToList();

        // Load all products at once
        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.ProductId))
            .ToDictionaryAsync(p => p.ProductId, cancellationToken);

        // Validate all products exist and are active
        foreach (var saleItem in request.SaleItems)
        {
            if (!products.TryGetValue(saleItem.ProductId, out var product))
            {
                throw new KeyNotFoundException($"Product with ID '{saleItem.ProductId}' not found");
            }

            if (!product.IsActive)
            {
                throw new InvalidOperationException($"Product '{product.Name}' is not active and cannot be sold");
            }

            if (product.StockQuantity < saleItem.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {saleItem.Quantity}");
            }
        }

        // Calculate total amount using product prices
        var totalAmount = request.SaleItems.Sum(item =>
        {
            var product = products[item.ProductId];
            return item.Quantity * product.UnitPrice;
        });

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

        // Create sales and update product stock
        var sales = new List<Sale>();
        foreach (var saleItem in request.SaleItems)
        {
            var product = products[saleItem.ProductId];

            var sale = new Sale
            {
                SaleId = Guid.NewGuid(),
                OrderId = order.OrderId,
                ProductId = saleItem.ProductId,
                ProductName = product.Name,
                Quantity = saleItem.Quantity,
                UnitPrice = product.UnitPrice,
                TotalPrice = saleItem.Quantity * product.UnitPrice,
                CreatedAt = DateTime.UtcNow
            };
            sales.Add(sale);
            _dbContext.Sales.Add(sale);

            // Decrease product stock
            product.StockQuantity -= saleItem.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
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
                s.ProductId,
                s.ProductName,
                s.Quantity,
                s.UnitPrice,
                s.TotalPrice
            )).ToList()
        );
    }
}
