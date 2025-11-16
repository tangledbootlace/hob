using FluentAssertions;
using HOB.Data.Entities;

namespace HOB.Data.Tests.Entities;

public class CustomerTests
{
    [Fact]
    public void Customer_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var customer = new Customer();

        // Assert
        customer.CustomerId.Should().Be(Guid.Empty);
        customer.Name.Should().Be(string.Empty);
        customer.Email.Should().Be(string.Empty);
        customer.Phone.Should().BeNull();
        customer.CreatedAt.Should().Be(default(DateTime));
        customer.UpdatedAt.Should().Be(default(DateTime));
        customer.Orders.Should().NotBeNull();
        customer.Orders.Should().BeEmpty();
    }

    [Fact]
    public void Customer_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow;

        // Act
        var customer = new Customer
        {
            CustomerId = customerId,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Phone = "555-1234",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        customer.CustomerId.Should().Be(customerId);
        customer.Name.Should().Be("John Doe");
        customer.Email.Should().Be("john.doe@example.com");
        customer.Phone.Should().Be("555-1234");
        customer.CreatedAt.Should().Be(createdAt);
        customer.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void Customer_ShouldAllowNullPhone()
    {
        // Arrange & Act
        var customer = new Customer
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            Phone = null
        };

        // Assert
        customer.Phone.Should().BeNull();
    }

    [Fact]
    public void Customer_ShouldSupportOrdersNavigation()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "test@example.com"
        };

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = customer.CustomerId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100.00m,
            Status = "Pending"
        };

        // Act
        customer.Orders.Add(order);

        // Assert
        customer.Orders.Should().HaveCount(1);
        customer.Orders.First().Should().Be(order);
    }
}
