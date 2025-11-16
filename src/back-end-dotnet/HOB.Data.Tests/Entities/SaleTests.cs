using FluentAssertions;
using HOB.Data.Entities;

namespace HOB.Data.Tests.Entities;

public class SaleTests
{
    [Fact]
    public void Sale_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var sale = new Sale();

        // Assert
        sale.SaleId.Should().Be(Guid.Empty);
        sale.OrderId.Should().Be(Guid.Empty);
        sale.ProductName.Should().Be(string.Empty);
        sale.Quantity.Should().Be(0);
        sale.UnitPrice.Should().Be(0);
        sale.TotalPrice.Should().Be(0);
        sale.CreatedAt.Should().Be(default(DateTime));
    }

    [Fact]
    public void Sale_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var saleId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var sale = new Sale
        {
            SaleId = saleId,
            OrderId = orderId,
            ProductName = "Laptop",
            Quantity = 3,
            UnitPrice = 999.99m,
            TotalPrice = 2999.97m,
            CreatedAt = createdAt
        };

        // Assert
        sale.SaleId.Should().Be(saleId);
        sale.OrderId.Should().Be(orderId);
        sale.ProductName.Should().Be("Laptop");
        sale.Quantity.Should().Be(3);
        sale.UnitPrice.Should().Be(999.99m);
        sale.TotalPrice.Should().Be(2999.97m);
        sale.CreatedAt.Should().Be(createdAt);
    }

    [Theory]
    [InlineData(1, 10.00, 10.00)]
    [InlineData(2, 25.50, 51.00)]
    [InlineData(5, 100.00, 500.00)]
    [InlineData(10, 7.99, 79.90)]
    public void Sale_ShouldCalculateTotalPriceCorrectly(int quantity, decimal unitPrice, decimal expectedTotal)
    {
        // Arrange & Act
        var sale = new Sale
        {
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = quantity * unitPrice
        };

        // Assert
        sale.TotalPrice.Should().Be(expectedTotal);
    }

    [Fact]
    public void Sale_ShouldSupportOrderNavigation()
    {
        // Arrange
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100.00m
        };

        var sale = new Sale
        {
            SaleId = Guid.NewGuid(),
            OrderId = order.OrderId,
            ProductName = "Test Product",
            Quantity = 1,
            UnitPrice = 100.00m,
            TotalPrice = 100.00m,
            Order = order
        };

        // Act & Assert
        sale.Order.Should().Be(order);
        sale.Order.OrderId.Should().Be(order.OrderId);
    }
}
