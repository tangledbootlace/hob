using FluentAssertions;
using HOB.API.Orders.CreateOrder;
using HOB.Data;
using HOB.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HOB.API.Tests.Orders;

public class CreateOrderRequestHandlerTests : IDisposable
{
    private readonly HobDbContext _context;
    private readonly Mock<ILogger<CreateOrderRequestHandler>> _loggerMock;
    private readonly CreateOrderRequestHandler _handler;

    public CreateOrderRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<HobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HobDbContext(options);
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<CreateOrderRequestHandler>>();
        _handler = new CreateOrderRequestHandler(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldCreateOrderWithSalesAndDecrementStock()
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

        var product1Id = Guid.NewGuid();
        var product1 = new Product
        {
            ProductId = product1Id,
            SKU = "PROD-1",
            Name = "Product 1",
            UnitPrice = 10.00m,
            StockQuantity = 100,
            LowStockThreshold = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var product2Id = Guid.NewGuid();
        var product2 = new Product
        {
            ProductId = product2Id,
            SKU = "PROD-2",
            Name = "Product 2",
            UnitPrice = 20.00m,
            StockQuantity = 50,
            LowStockThreshold = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            CustomerId: customerId,
            OrderDate: DateTime.UtcNow,
            Status: "Pending",
            SaleItems: new List<SaleItemRequest>
            {
                new SaleItemRequest(product1Id, 5),
                new SaleItemRequest(product2Id, 3)
            }
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.CustomerId.Should().Be(customerId);
        response.Status.Should().Be("Pending");
        response.TotalAmount.Should().Be(110.00m); // (5 * 10) + (3 * 20)
        response.Sales.Should().HaveCount(2);

        // Verify stock was decremented
        var updatedProduct1 = await _context.Products.FindAsync(product1Id);
        updatedProduct1!.StockQuantity.Should().Be(95);

        var updatedProduct2 = await _context.Products.FindAsync(product2Id);
        updatedProduct2!.StockQuantity.Should().Be(47);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenCustomerNotFound()
    {
        // Arrange
        var nonExistentCustomerId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            CustomerId: nonExistentCustomerId,
            OrderDate: DateTime.UtcNow,
            Status: "Pending",
            SaleItems: new List<SaleItemRequest>()
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Customer with ID '{nonExistentCustomerId}' not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenNoSaleItems()
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

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            CustomerId: customerId,
            OrderDate: DateTime.UtcNow,
            Status: "Pending",
            SaleItems: new List<SaleItemRequest>()
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Order must have at least one sale item");
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

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var nonExistentProductId = Guid.NewGuid();
        var request = new CreateOrderRequest(
            CustomerId: customerId,
            OrderDate: DateTime.UtcNow,
            Status: "Pending",
            SaleItems: new List<SaleItemRequest>
            {
                new SaleItemRequest(nonExistentProductId, 1)
            }
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

        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "INACTIVE",
            Name = "Inactive Product",
            UnitPrice = 10.00m,
            StockQuantity = 100,
            LowStockThreshold = 10,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            CustomerId: customerId,
            OrderDate: DateTime.UtcNow,
            Status: "Pending",
            SaleItems: new List<SaleItemRequest>
            {
                new SaleItemRequest(productId, 1)
            }
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
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            CustomerId: customerId,
            OrderDate: DateTime.UtcNow,
            Status: "Pending",
            SaleItems: new List<SaleItemRequest>
            {
                new SaleItemRequest(productId, 10)
            }
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Insufficient stock for product 'Low Stock Product'. Available: 5, Requested: 10");
    }

    [Fact]
    public async Task Handle_ShouldValidateStockForAllProducts_BeforeCreating()
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

        var product1Id = Guid.NewGuid();
        var product1 = new Product
        {
            ProductId = product1Id,
            SKU = "STOCK-OK",
            Name = "Stock OK Product",
            UnitPrice = 10.00m,
            StockQuantity = 100,
            LowStockThreshold = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var product2Id = Guid.NewGuid();
        var product2 = new Product
        {
            ProductId = product2Id,
            SKU = "NO-STOCK",
            Name = "No Stock Product",
            UnitPrice = 20.00m,
            StockQuantity = 0,
            LowStockThreshold = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            CustomerId: customerId,
            OrderDate: DateTime.UtcNow,
            Status: "Pending",
            SaleItems: new List<SaleItemRequest>
            {
                new SaleItemRequest(product1Id, 1),
                new SaleItemRequest(product2Id, 1) // This will fail
            }
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Insufficient stock for product 'No Stock Product'. Available: 0, Requested: 1");

        // Verify no stock was decremented (transaction should rollback)
        var product1AfterFail = await _context.Products.FindAsync(product1Id);
        product1AfterFail!.StockQuantity.Should().Be(100);
    }

    [Fact]
    public async Task Handle_ShouldCopyProductNamesToSales()
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

        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "NAME-COPY",
            Name = "Product Name to Copy",
            UnitPrice = 10.00m,
            StockQuantity = 100,
            LowStockThreshold = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest(
            CustomerId: customerId,
            OrderDate: DateTime.UtcNow,
            Status: "Pending",
            SaleItems: new List<SaleItemRequest>
            {
                new SaleItemRequest(productId, 1)
            }
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Sales.First().ProductName.Should().Be("Product Name to Copy");
    }
}
