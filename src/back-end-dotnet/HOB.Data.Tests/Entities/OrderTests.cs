using FluentAssertions;
using HOB.Data.Entities;

namespace HOB.Data.Tests.Entities;

public class OrderTests
{
    [Fact]
    public void Order_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var order = new Order();

        // Assert
        order.OrderId.Should().Be(Guid.Empty);
        order.CustomerId.Should().Be(Guid.Empty);
        order.OrderDate.Should().Be(default(DateTime));
        order.TotalAmount.Should().Be(0);
        order.Status.Should().Be("Pending");
        order.CreatedAt.Should().Be(default(DateTime));
        order.UpdatedAt.Should().Be(default(DateTime));
        order.Sales.Should().NotBeNull();
        order.Sales.Should().BeEmpty();
    }

    [Fact]
    public void Order_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var orderDate = DateTime.UtcNow;
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var order = new Order
        {
            OrderId = orderId,
            CustomerId = customerId,
            OrderDate = orderDate,
            TotalAmount = 250.75m,
            Status = "Completed",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        order.OrderId.Should().Be(orderId);
        order.CustomerId.Should().Be(customerId);
        order.OrderDate.Should().Be(orderDate);
        order.TotalAmount.Should().Be(250.75m);
        order.Status.Should().Be("Completed");
        order.CreatedAt.Should().Be(createdAt);
        order.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void Order_ShouldSupportSalesNavigation()
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
            Quantity = 2,
            UnitPrice = 50.00m,
            TotalPrice = 100.00m
        };

        // Act
        order.Sales.Add(sale);

        // Assert
        order.Sales.Should().HaveCount(1);
        order.Sales.First().Should().Be(sale);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Processing")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public void Order_ShouldSupportDifferentStatuses(string status)
    {
        // Arrange & Act
        var order = new Order { Status = status };

        // Assert
        order.Status.Should().Be(status);
    }
}
