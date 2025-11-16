using FluentAssertions;
using HOB.API.Products.GetProduct;
using HOB.Data;
using HOB.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Tests.Products;

public class GetProductRequestHandlerTests : IDisposable
{
    private readonly HobDbContext _context;
    private readonly GetProductRequestHandler _handler;

    public GetProductRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<HobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HobDbContext(options);
        _context.Database.EnsureCreated();

        _handler = new GetProductRequestHandler(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "GET-TEST-001",
            Name = "Get Test Product",
            Description = "Test Description",
            UnitPrice = 15.00m,
            StockQuantity = 50,
            LowStockThreshold = 5,
            Category = "Test Category",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new GetProductRequest(productId);

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.ProductId.Should().Be(productId);
        response.SKU.Should().Be("GET-TEST-001");
        response.Name.Should().Be("Get Test Product");
        response.Description.Should().Be("Test Description");
        response.UnitPrice.Should().Be(15.00m);
        response.StockQuantity.Should().Be(50);
        response.LowStockThreshold.Should().Be(5);
        response.Category.Should().Be("Test Category");
        response.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenProductDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new GetProductRequest(nonExistentId);

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Product with ID '{nonExistentId}' not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnProductWithNullOptionalFields()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "NULL-FIELDS",
            Name = "Null Fields Product",
            Description = null,
            UnitPrice = 10.00m,
            StockQuantity = 20,
            LowStockThreshold = 2,
            Category = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new GetProductRequest(productId);

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Description.Should().BeNull();
        response.Category.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnInactiveProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            ProductId = productId,
            SKU = "INACTIVE-TEST",
            Name = "Inactive Product",
            UnitPrice = 10.00m,
            StockQuantity = 0,
            LowStockThreshold = 5,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var request = new GetProductRequest(productId);

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.IsActive.Should().BeFalse();
    }
}
