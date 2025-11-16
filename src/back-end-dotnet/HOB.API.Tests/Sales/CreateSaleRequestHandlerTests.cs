using FluentAssertions;
using HOB.API.Sales.CreateSale;
using HOB.Data;
using HOB.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HOB.API.Tests.Sales;

public class CreateSaleRequestHandlerTests : IDisposable
{
    private readonly HobDbContext _context;
    private readonly Mock<ILogger<CreateSaleRequestHandler>> _loggerMock;
    private readonly CreateSaleRequestHandler _handler;

    public CreateSaleRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<HobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HobDbContext(options);
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<CreateSaleRequestHandler>>();
        _handler = new CreateSaleRequestHandler(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldCreateSaleAndDecrementStock_WhenValidRequest()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            CustomerId = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orderId = Guid.NewGuid();
        var order = new Order
        {
            OrderId = orderId,
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 0m,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "SALE-TEST-001",
            Name = "Sale Test Product",
            UnitPrice = 25.00m,
            StockQuantity = 100,
            LowStockThreshold = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Orders.Add(order);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new CreateSaleRequest(
            OrderId: orderId,
            ProductId: productId,
            Quantity: 5
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.ProductId.Should().Be(productId);
        response.ProductName.Should().Be("Sale Test Product");
        response.Quantity.Should().Be(5);
        response.UnitPrice.Should().Be(25.00m);
        response.TotalPrice.Should().Be(125.00m);

        // Verify stock was decremented
        var updatedProduct = await _context.Products.FindAsync(productId);
        updatedProduct!.StockQuantity.Should().Be(95);

        // Verify order total was updated
        var updatedOrder = await _context.Orders.FindAsync(orderId);
        updatedOrder!.TotalAmount.Should().Be(125.00m);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenOrderNotFound()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var request = new CreateSaleRequest(
            OrderId: nonExistentOrderId,
            ProductId: productId,
            Quantity: 1
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with ID '{nonExistentOrderId}' not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenProductNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            CustomerId = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orderId = Guid.NewGuid();
        var order = new Order
        {
            OrderId = orderId,
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 0m,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var nonExistentProductId = Guid.NewGuid();
        var request = new CreateSaleRequest(
            OrderId: orderId,
            ProductId: nonExistentProductId,
            Quantity: 1
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Product with ID '{nonExistentProductId}' not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenProductIsNotActive()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            CustomerId = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orderId = Guid.NewGuid();
        var order = new Order
        {
            OrderId = orderId,
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 0m,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "INACTIVE-PROD",
            Name = "Inactive Product",
            UnitPrice = 10.00m,
            StockQuantity = 100,
            LowStockThreshold = 10,
            IsActive = false, // Inactive
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Orders.Add(order);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new CreateSaleRequest(
            OrderId: orderId,
            ProductId: productId,
            Quantity: 1
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Product 'Inactive Product' is not active and cannot be sold");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenInsufficientStock()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            CustomerId = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orderId = Guid.NewGuid();
        var order = new Order
        {
            OrderId = orderId,
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 0m,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "LOW-STOCK",
            Name = "Low Stock Product",
            UnitPrice = 10.00m,
            StockQuantity = 5,
            LowStockThreshold = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Orders.Add(order);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new CreateSaleRequest(
            OrderId: orderId,
            ProductId: productId,
            Quantity: 10 // More than available
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Insufficient stock for product 'Low Stock Product'. Available: 5, Requested: 10");
    }

    [Fact]
    public async Task Handle_ShouldAllowSaleWithExactlyAvailableStock()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            CustomerId = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orderId = Guid.NewGuid();
        var order = new Order
        {
            OrderId = orderId,
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 0m,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "EXACT-STOCK",
            Name = "Exact Stock Product",
            UnitPrice = 10.00m,
            StockQuantity = 10,
            LowStockThreshold = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Orders.Add(order);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new CreateSaleRequest(
            OrderId: orderId,
            ProductId: productId,
            Quantity: 10 // Exactly available stock
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        var updatedProduct = await _context.Products.FindAsync(productId);
        updatedProduct!.StockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldCopyProductNameToSale()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            CustomerId = customerId,
            Name = "Test Customer",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orderId = Guid.NewGuid();
        var order = new Order
        {
            OrderId = orderId,
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 0m,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "NAME-TEST",
            Name = "Product Name for Historical Data",
            UnitPrice = 10.00m,
            StockQuantity = 100,
            LowStockThreshold = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Orders.Add(order);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new CreateSaleRequest(
            OrderId: orderId,
            ProductId: productId,
            Quantity: 1
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.ProductName.Should().Be("Product Name for Historical Data");

        var sale = await _context.Sales.FindAsync(response.SaleId);
        sale!.ProductName.Should().Be("Product Name for Historical Data");
    }
}
