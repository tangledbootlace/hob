using FluentAssertions;
using HOB.API.Products.DeleteProduct;
using HOB.Data;
using HOB.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HOB.API.Tests.Products;

public class DeleteProductRequestHandlerTests : IDisposable
{
    private readonly HobDbContext _context;
    private readonly Mock<ILogger<DeleteProductRequestHandler>> _loggerMock;
    private readonly DeleteProductRequestHandler _handler;

    public DeleteProductRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<HobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HobDbContext(options);
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<DeleteProductRequestHandler>>();
        _handler = new DeleteProductRequestHandler(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldDeleteProduct_WhenNoSalesExist()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "DELETE-001",
            Name = "Product to Delete",
            UnitPrice = 10.00m,
            StockQuantity = 10,
            LowStockThreshold = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new DeleteProductRequest(productId);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        var deletedProduct = await _context.Products.FindAsync(productId);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenProductNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new DeleteProductRequest(nonExistentId);

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Product with ID '{nonExistentId}' not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenProductHasSales()
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
            TotalAmount = 50.00m,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "HAS-SALES",
            Name = "Product with Sales",
            UnitPrice = 25.00m,
            StockQuantity = 10,
            LowStockThreshold = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var sale = new Sale
        {
            SaleId = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            ProductName = "Product with Sales",
            Quantity = 2,
            UnitPrice = 25.00m,
            TotalPrice = 50.00m,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Orders.Add(order);
        _context.Products.Add(product);
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var request = new DeleteProductRequest(productId);

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot delete product with existing sales. Product has 1 sale(s). Consider marking it as inactive instead.");
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenProductDeleted()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "LOG-DELETE",
            Name = "Log Delete Test",
            UnitPrice = 10.00m,
            StockQuantity = 10,
            LowStockThreshold = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new DeleteProductRequest(productId);

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleted product")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
