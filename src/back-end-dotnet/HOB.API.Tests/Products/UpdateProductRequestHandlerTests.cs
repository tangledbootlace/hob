using FluentAssertions;
using HOB.API.Products.UpdateProduct;
using HOB.Data;
using HOB.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HOB.API.Tests.Products;

public class UpdateProductRequestHandlerTests : IDisposable
{
    private readonly HobDbContext _context;
    private readonly Mock<ILogger<UpdateProductRequestHandler>> _loggerMock;
    private readonly UpdateProductRequestHandler _handler;

    public UpdateProductRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<HobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HobDbContext(options);
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<UpdateProductRequestHandler>>();
        _handler = new UpdateProductRequestHandler(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldUpdateProduct_WhenValidRequest()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "UPDATE-001",
            Name = "Original Name",
            Description = "Original Description",
            UnitPrice = 10.00m,
            StockQuantity = 100,
            LowStockThreshold = 10,
            Category = "Original Category",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new UpdateProductRequest(
            ProductId: productId,
            SKU: "UPDATE-002",
            Name: "Updated Name",
            Description: "Updated Description",
            UnitPrice: 20.00m,
            StockQuantity: 50,
            LowStockThreshold: 5,
            Category: "Updated Category",
            IsActive: true
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.SKU.Should().Be("UPDATE-002");
        response.Name.Should().Be("Updated Name");
        response.Description.Should().Be("Updated Description");
        response.UnitPrice.Should().Be(20.00m);
        response.StockQuantity.Should().Be(50);
        response.LowStockThreshold.Should().Be(5);
        response.Category.Should().Be("Updated Category");
        response.IsActive.Should().BeTrue();
        response.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenProductNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new UpdateProductRequest(
            ProductId: nonExistentId,
            SKU: "TEST",
            Name: "Test",
            Description: "Test",
            UnitPrice: 10.00m,
            StockQuantity: 10,
            LowStockThreshold: 1,
            Category: "Test",
            IsActive: true
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Product with ID '{nonExistentId}' not found");
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenSKUAlreadyExistsForDifferentProduct()
    {
        // Arrange
        var product1Id = Guid.NewGuid();
        var product1 = new Product
        {
            ProductId = product1Id,
            SKU = "PRODUCT-1",
            Name = "Product 1",
            UnitPrice = 10.00m,
            StockQuantity = 10,
            LowStockThreshold = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var product2Id = Guid.NewGuid();
        var product2 = new Product
        {
            ProductId = product2Id,
            SKU = "PRODUCT-2",
            Name = "Product 2",
            UnitPrice = 20.00m,
            StockQuantity = 20,
            LowStockThreshold = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        var request = new UpdateProductRequest(
            ProductId: product2Id,
            SKU: "PRODUCT-1", // Try to use product1's SKU
            Name: "Updated Product 2",
            Description: null,
            UnitPrice: 25.00m,
            StockQuantity: 15,
            LowStockThreshold: 3,
            Category: null,
            IsActive: true
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Product with SKU 'PRODUCT-1' already exists");
    }

    [Fact]
    public async Task Handle_ShouldAllowSameSKU_ForSameProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "SAME-SKU",
            Name = "Original Name",
            UnitPrice = 10.00m,
            StockQuantity = 10,
            LowStockThreshold = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new UpdateProductRequest(
            ProductId: productId,
            SKU: "SAME-SKU", // Keep same SKU
            Name: "Updated Name",
            Description: null,
            UnitPrice: 20.00m,
            StockQuantity: 20,
            LowStockThreshold: 2,
            Category: null,
            IsActive: true
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.SKU.Should().Be("SAME-SKU");
        response.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task Handle_ShouldUpdateIsActiveStatus()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "ACTIVE-TEST",
            Name = "Test Product",
            UnitPrice = 10.00m,
            StockQuantity = 10,
            LowStockThreshold = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new UpdateProductRequest(
            ProductId: productId,
            SKU: "ACTIVE-TEST",
            Name: "Test Product",
            Description: null,
            UnitPrice: 10.00m,
            StockQuantity: 10,
            LowStockThreshold: 1,
            Category: null,
            IsActive: false // Deactivate
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.IsActive.Should().BeFalse();

        var updatedProduct = await _context.Products.FindAsync(productId);
        updatedProduct!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenProductUpdated()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "LOG-TEST",
            Name = "Log Test",
            UnitPrice = 10.00m,
            StockQuantity = 10,
            LowStockThreshold = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new UpdateProductRequest(
            ProductId: productId,
            SKU: "LOG-TEST",
            Name: "Updated Name",
            Description: null,
            UnitPrice: 15.00m,
            StockQuantity: 15,
            LowStockThreshold: 2,
            Category: null,
            IsActive: true
        );

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Updated product")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
