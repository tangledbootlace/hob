using FluentAssertions;
using HOB.API.Products.CreateProduct;
using HOB.Data;
using HOB.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HOB.API.Tests.Products;

public class CreateProductRequestHandlerTests : IDisposable
{
    private readonly HobDbContext _context;
    private readonly Mock<ILogger<CreateProductRequestHandler>> _loggerMock;
    private readonly CreateProductRequestHandler _handler;

    public CreateProductRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<HobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HobDbContext(options);
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<CreateProductRequestHandler>>();
        _handler = new CreateProductRequestHandler(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldCreateProduct_WhenValidRequest()
    {
        // Arrange
        var request = new CreateProductRequest(
            SKU: "TEST-001",
            Name: "Test Product",
            Description: "Test Description",
            UnitPrice: 10.00m,
            StockQuantity: 100,
            LowStockThreshold: 10,
            Category: "Test Category"
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.SKU.Should().Be("TEST-001");
        response.Name.Should().Be("Test Product");
        response.Description.Should().Be("Test Description");
        response.UnitPrice.Should().Be(10.00m);
        response.StockQuantity.Should().Be(100);
        response.LowStockThreshold.Should().Be(10);
        response.Category.Should().Be("Test Category");
        response.IsActive.Should().BeTrue();
        response.ProductId.Should().NotBe(Guid.Empty);
        response.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        response.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldSaveProductToDatabase_WhenValidRequest()
    {
        // Arrange
        var request = new CreateProductRequest(
            SKU: "SKU-002",
            Name: "Another Product",
            Description: null,
            UnitPrice: 25.50m,
            StockQuantity: 50,
            LowStockThreshold: 5,
            Category: null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        var product = await _context.Products.FindAsync(response.ProductId);
        product.Should().NotBeNull();
        product!.SKU.Should().Be("SKU-002");
        product.Name.Should().Be("Another Product");
        product.Description.Should().BeNull();
        product.UnitPrice.Should().Be(25.50m);
        product.StockQuantity.Should().Be(50);
        product.LowStockThreshold.Should().Be(5);
        product.Category.Should().BeNull();
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenSKUAlreadyExists()
    {
        // Arrange
        var existingProduct = new Product
        {
            ProductId = Guid.NewGuid(),
            SKU = "DUPLICATE-SKU",
            Name = "Existing Product",
            UnitPrice = 10.00m,
            StockQuantity = 10,
            LowStockThreshold = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Products.Add(existingProduct);
        await _context.SaveChangesAsync();

        var request = new CreateProductRequest(
            SKU: "DUPLICATE-SKU",
            Name: "New Product",
            Description: "Description",
            UnitPrice: 20.00m,
            StockQuantity: 20,
            LowStockThreshold: 2,
            Category: "Category"
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Product with SKU 'DUPLICATE-SKU' already exists");
    }

    [Fact]
    public async Task Handle_ShouldAllowNullDescriptionAndCategory()
    {
        // Arrange
        var request = new CreateProductRequest(
            SKU: "NULL-TEST",
            Name: "Null Test Product",
            Description: null,
            UnitPrice: 5.00m,
            StockQuantity: 100,
            LowStockThreshold: 10,
            Category: null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Description.Should().BeNull();
        response.Category.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldSetIsActiveToTrue_ByDefault()
    {
        // Arrange
        var request = new CreateProductRequest(
            SKU: "ACTIVE-TEST",
            Name: "Active Test",
            Description: "Test",
            UnitPrice: 10.00m,
            StockQuantity: 50,
            LowStockThreshold: 5,
            Category: "Test"
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.IsActive.Should().BeTrue();

        var product = await _context.Products.FindAsync(response.ProductId);
        product!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenProductCreated()
    {
        // Arrange
        var request = new CreateProductRequest(
            SKU: "LOG-TEST",
            Name: "Log Test Product",
            Description: "Test",
            UnitPrice: 15.00m,
            StockQuantity: 75,
            LowStockThreshold: 7,
            Category: "Test"
        );

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created product")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHandleDecimalPrices_Correctly()
    {
        // Arrange
        var request = new CreateProductRequest(
            SKU: "DECIMAL-TEST",
            Name: "Decimal Test",
            Description: "Test",
            UnitPrice: 99.99m,
            StockQuantity: 10,
            LowStockThreshold: 1,
            Category: "Test"
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.UnitPrice.Should().Be(99.99m);

        var product = await _context.Products.FindAsync(response.ProductId);
        product!.UnitPrice.Should().Be(99.99m);
    }

    [Fact]
    public async Task Handle_ShouldHandleZeroStockQuantity()
    {
        // Arrange
        var request = new CreateProductRequest(
            SKU: "ZERO-STOCK",
            Name: "Zero Stock Product",
            Description: "Test",
            UnitPrice: 10.00m,
            StockQuantity: 0,
            LowStockThreshold: 10,
            Category: "Test"
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.StockQuantity.Should().Be(0);
    }
}
