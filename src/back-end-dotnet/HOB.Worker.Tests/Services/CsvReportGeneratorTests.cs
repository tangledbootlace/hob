using FluentAssertions;
using HOB.Worker.Services;

namespace HOB.Worker.Tests.Services;

public class CsvReportGeneratorTests
{
    private readonly CsvReportGenerator _generator;

    public CsvReportGeneratorTests()
    {
        _generator = new CsvReportGenerator();
    }

    [Fact]
    public void GenerateSalesReport_ShouldIncludeHeader()
    {
        // Arrange
        var data = Enumerable.Empty<ReportDataRow>();

        // Act
        var result = _generator.GenerateSalesReport(data);

        // Assert
        result.Should().Contain("Customer Name,Customer Email,Customer Phone,Order ID,Order Date,Order Total,Order Status,Product Name,Quantity,Unit Price,Line Total");
    }

    [Fact]
    public void GenerateSalesReport_ShouldGenerateEmptyReport_WhenNoData()
    {
        // Arrange
        var data = Enumerable.Empty<ReportDataRow>();

        // Act
        var result = _generator.GenerateSalesReport(data);

        // Assert
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(1); // Only header
    }

    [Fact]
    public void GenerateSalesReport_ShouldGenerateSingleRow()
    {
        // Arrange
        var data = new[]
        {
            new ReportDataRow(
                CustomerName: "John Doe",
                CustomerEmail: "john@example.com",
                CustomerPhone: "555-1234",
                OrderId: Guid.Parse("10000000-1000-1000-1000-100000000001"),
                OrderDate: new DateTime(2025, 1, 15, 14, 30, 0, DateTimeKind.Utc),
                OrderTotal: 100.00m,
                OrderStatus: "Completed",
                ProductName: "Widget A",
                Quantity: 2,
                UnitPrice: 50.00m,
                LineTotal: 100.00m
            )
        };

        // Act
        var result = _generator.GenerateSalesReport(data);

        // Assert
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(2); // Header + 1 data row
        lines[1].Should().Contain("John Doe");
        lines[1].Should().Contain("john@example.com");
        lines[1].Should().Contain("555-1234");
        lines[1].Should().Contain("Widget A");
    }

    [Fact]
    public void GenerateSalesReport_ShouldFormatDecimalsCorrectly()
    {
        // Arrange
        var data = new[]
        {
            new ReportDataRow(
                CustomerName: "Test User",
                CustomerEmail: "test@example.com",
                CustomerPhone: null,
                OrderId: Guid.NewGuid(),
                OrderDate: DateTime.UtcNow,
                OrderTotal: 123.45m,
                OrderStatus: "Pending",
                ProductName: "Test Product",
                Quantity: 1,
                UnitPrice: 123.45m,
                LineTotal: 123.45m
            )
        };

        // Act
        var result = _generator.GenerateSalesReport(data);

        // Assert
        result.Should().Contain("123.45");
    }

    [Fact]
    public void GenerateSalesReport_ShouldFormatDateCorrectly()
    {
        // Arrange
        var orderDate = new DateTime(2025, 1, 15, 14, 30, 45, DateTimeKind.Utc);
        var data = new[]
        {
            new ReportDataRow(
                CustomerName: "Test User",
                CustomerEmail: "test@example.com",
                CustomerPhone: null,
                OrderId: Guid.NewGuid(),
                OrderDate: orderDate,
                OrderTotal: 100.00m,
                OrderStatus: "Completed",
                ProductName: "Test Product",
                Quantity: 1,
                UnitPrice: 100.00m,
                LineTotal: 100.00m
            )
        };

        // Act
        var result = _generator.GenerateSalesReport(data);

        // Assert
        result.Should().Contain("2025-01-15 14:30:45");
    }

    [Fact]
    public void GenerateSalesReport_ShouldHandleNullPhone()
    {
        // Arrange
        var data = new[]
        {
            new ReportDataRow(
                CustomerName: "Test User",
                CustomerEmail: "test@example.com",
                CustomerPhone: null,
                OrderId: Guid.NewGuid(),
                OrderDate: DateTime.UtcNow,
                OrderTotal: 100.00m,
                OrderStatus: "Pending",
                ProductName: "Test Product",
                Quantity: 1,
                UnitPrice: 100.00m,
                LineTotal: 100.00m
            )
        };

        // Act
        var result = _generator.GenerateSalesReport(data);

        // Assert
        result.Should().NotBeNull();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(2);
    }

    [Fact]
    public void GenerateSalesReport_ShouldEscapeCommasInValues()
    {
        // Arrange
        var data = new[]
        {
            new ReportDataRow(
                CustomerName: "Doe, John",
                CustomerEmail: "john@example.com",
                CustomerPhone: "555-1234",
                OrderId: Guid.NewGuid(),
                OrderDate: DateTime.UtcNow,
                OrderTotal: 100.00m,
                OrderStatus: "Completed",
                ProductName: "Widget A, Premium",
                Quantity: 1,
                UnitPrice: 100.00m,
                LineTotal: 100.00m
            )
        };

        // Act
        var result = _generator.GenerateSalesReport(data);

        // Assert
        result.Should().Contain("\"Doe, John\"");
        result.Should().Contain("\"Widget A, Premium\"");
    }

    [Fact]
    public void GenerateSalesReport_ShouldEscapeQuotesInValues()
    {
        // Arrange
        var data = new[]
        {
            new ReportDataRow(
                CustomerName: "John \"The Boss\" Doe",
                CustomerEmail: "john@example.com",
                CustomerPhone: null,
                OrderId: Guid.NewGuid(),
                OrderDate: DateTime.UtcNow,
                OrderTotal: 100.00m,
                OrderStatus: "Completed",
                ProductName: "Test Product",
                Quantity: 1,
                UnitPrice: 100.00m,
                LineTotal: 100.00m
            )
        };

        // Act
        var result = _generator.GenerateSalesReport(data);

        // Assert
        result.Should().Contain("\"John \"\"The Boss\"\" Doe\"");
    }

    [Fact]
    public void GenerateSalesReport_ShouldHandleMultipleRows()
    {
        // Arrange
        var data = new[]
        {
            new ReportDataRow(
                CustomerName: "Customer 1",
                CustomerEmail: "customer1@example.com",
                CustomerPhone: "555-0001",
                OrderId: Guid.NewGuid(),
                OrderDate: DateTime.UtcNow,
                OrderTotal: 100.00m,
                OrderStatus: "Completed",
                ProductName: "Product 1",
                Quantity: 1,
                UnitPrice: 100.00m,
                LineTotal: 100.00m
            ),
            new ReportDataRow(
                CustomerName: "Customer 2",
                CustomerEmail: "customer2@example.com",
                CustomerPhone: "555-0002",
                OrderId: Guid.NewGuid(),
                OrderDate: DateTime.UtcNow,
                OrderTotal: 200.00m,
                OrderStatus: "Pending",
                ProductName: "Product 2",
                Quantity: 2,
                UnitPrice: 100.00m,
                LineTotal: 200.00m
            )
        };

        // Act
        var result = _generator.GenerateSalesReport(data);

        // Assert
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(3); // Header + 2 data rows
        result.Should().Contain("Customer 1");
        result.Should().Contain("Customer 2");
        result.Should().Contain("Product 1");
        result.Should().Contain("Product 2");
    }
}
