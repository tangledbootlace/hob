using FluentAssertions;
using HOB.API.Products.ListProducts;
using HOB.Data;
using HOB.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Tests.Products;

public class ListProductsRequestHandlerTests : IDisposable
{
    private readonly HobDbContext _context;
    private readonly ListProductsRequestHandler _handler;

    public ListProductsRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<HobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HobDbContext(options);
        _context.Database.EnsureCreated();

        _handler = new ListProductsRequestHandler(_context);

        // Seed test data
        SeedTestData();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private void SeedTestData()
    {
        var products = new List<Product>
        {
            new Product
            {
                ProductId = Guid.NewGuid(),
                SKU = "LAPTOP-001",
                Name = "Gaming Laptop",
                Description = "High-performance gaming laptop",
                UnitPrice = 1500.00m,
                StockQuantity = 50,
                LowStockThreshold = 10,
                Category = "Electronics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                SKU = "MOUSE-001",
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse",
                UnitPrice = 25.00m,
                StockQuantity = 5,
                LowStockThreshold = 10,
                Category = "Electronics",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                SKU = "DESK-001",
                Name = "Standing Desk",
                Description = "Adjustable standing desk",
                UnitPrice = 500.00m,
                StockQuantity = 100,
                LowStockThreshold = 20,
                Category = "Furniture",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                SKU = "CHAIR-001",
                Name = "Office Chair",
                Description = "Ergonomic office chair",
                UnitPrice = 300.00m,
                StockQuantity = 0,
                LowStockThreshold = 5,
                Category = "Furniture",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_ShouldReturnAllProducts_WhenNoFiltersApplied()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 10,
            Search: null,
            Category: null,
            LowStock: null,
            ActiveOnly: null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Products.Should().HaveCount(4);
        response.TotalItems.Should().Be(4);
        response.TotalPages.Should().Be(1);
        response.Page.Should().Be(1);
        response.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldFilterActiveProducts_WhenActiveOnlyIsTrue()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 10,
            Search: null,
            Category: null,
            LowStock: null,
            ActiveOnly: true
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Products.Should().HaveCount(3);
        response.Products.Should().OnlyContain(p => p.IsActive);
        response.TotalItems.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ShouldSearchByName_WhenSearchProvided()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 10,
            Search: "Laptop",
            Category: null,
            LowStock: null,
            ActiveOnly: null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Products.Should().HaveCount(1);
        response.Products.First().Name.Should().Contain("Laptop");
    }

    [Fact]
    public async Task Handle_ShouldSearchBySKU_WhenSearchProvided()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 10,
            Search: "MOUSE-001",
            Category: null,
            LowStock: null,
            ActiveOnly: null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Products.Should().HaveCount(1);
        response.Products.First().SKU.Should().Be("MOUSE-001");
    }

    [Fact]
    public async Task Handle_ShouldSearchByDescription_WhenSearchProvided()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 10,
            Search: "Ergonomic",
            Category: null,
            LowStock: null,
            ActiveOnly: null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Products.Should().HaveCount(2); // Mouse and Chair
        response.Products.Should().OnlyContain(p => p.Description!.Contains("Ergonomic"));
    }

    [Fact]
    public async Task Handle_ShouldFilterByCategory_WhenCategoryProvided()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 10,
            Search: null,
            Category: "Electronics",
            LowStock: null,
            ActiveOnly: null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Products.Should().HaveCount(2);
        response.Products.Should().OnlyContain(p => p.Category == "Electronics");
    }

    [Fact]
    public async Task Handle_ShouldFilterLowStockProducts_WhenLowStockIsTrue()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 10,
            Search: null,
            Category: null,
            LowStock: true,
            ActiveOnly: null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Products.Should().HaveCount(2); // Mouse (5 <= 10) and Chair (0 <= 5)
        response.Products.Should().OnlyContain(p => p.IsLowStock);
    }

    [Fact]
    public async Task Handle_ShouldSetIsLowStockFlag_Correctly()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 10,
            Search: null,
            Category: null,
            LowStock: null,
            ActiveOnly: true
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        var laptop = response.Products.First(p => p.SKU == "LAPTOP-001");
        laptop.IsLowStock.Should().BeFalse(); // 50 > 10

        var mouse = response.Products.First(p => p.SKU == "MOUSE-001");
        mouse.IsLowStock.Should().BeTrue(); // 5 <= 10
    }

    [Fact]
    public async Task Handle_ShouldPaginateCorrectly()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 2,
            Search: null,
            Category: null,
            LowStock: null,
            ActiveOnly: null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Products.Should().HaveCount(2);
        response.TotalItems.Should().Be(4);
        response.TotalPages.Should().Be(2);
        response.Page.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnSecondPage_WhenPageIsTwoWithPageSizeTwo()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 2,
            PageSize: 2,
            Search: null,
            Category: null,
            LowStock: null,
            ActiveOnly: null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Products.Should().HaveCount(2);
        response.TotalItems.Should().Be(4);
        response.TotalPages.Should().Be(2);
        response.Page.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldOrderByName_Alphabetically()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 10,
            Search: null,
            Category: null,
            LowStock: null,
            ActiveOnly: true
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Products.Should().BeInAscendingOrder(p => p.Name);
    }

    [Fact]
    public async Task Handle_ShouldCombineMultipleFilters()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 10,
            Search: null,
            Category: "Electronics",
            LowStock: true,
            ActiveOnly: true
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Products.Should().HaveCount(1); // Only Mouse matches all criteria
        response.Products.First().SKU.Should().Be("MOUSE-001");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoProductsMatchFilters()
    {
        // Arrange
        var request = new ListProductsRequest(
            Page: 1,
            PageSize: 10,
            Search: "NonExistentProduct",
            Category: null,
            LowStock: null,
            ActiveOnly: null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Products.Should().BeEmpty();
        response.TotalItems.Should().Be(0);
        response.TotalPages.Should().Be(0);
    }
}
