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

        // Verify product exists and get product details
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);

        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID '{request.ProductId}' not found");
        }

        if (!product.IsActive)
        {
            throw new InvalidOperationException($"Product '{product.Name}' is not active and cannot be sold");
        }

        // Check if sufficient stock is available
        if (product.StockQuantity < request.Quantity)
        {
            throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {request.Quantity}");
        }

        // Calculate total price
        var totalPrice = request.Quantity * product.UnitPrice;

        var sale = new Sale
        {
            SaleId = Guid.NewGuid(),
            OrderId = request.OrderId,
            ProductId = request.ProductId,
            ProductName = product.Name,
            Quantity = request.Quantity,
            UnitPrice = product.UnitPrice,
            TotalPrice = totalPrice,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Sales.Add(sale);

        // Update order's total amount
        order.TotalAmount += totalPrice;
        order.UpdatedAt = DateTime.UtcNow;

        // Decrease product stock quantity
        product.StockQuantity -= request.Quantity;
        product.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created sale {SaleId} for order {OrderId} with total price {TotalPrice}. Product stock updated: {ProductId} - {StockQuantity} remaining",
            sale.SaleId, sale.OrderId, sale.TotalPrice, product.ProductId, product.StockQuantity);

        return new CreateSaleResponse(
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
