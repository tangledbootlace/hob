using FluentAssertions;
using HOB.API.Customers.GetCustomer;
using HOB.Data;
using HOB.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HOB.API.Tests.Customers;

public class GetCustomerRequestHandlerTests : IDisposable
{
    private readonly HobDbContext _context;
    private readonly GetCustomerRequestHandler _handler;

    public GetCustomerRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<HobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HobDbContext(options);
        _context.Database.EnsureCreated();

        _handler = new GetCustomerRequestHandler(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomer_WhenCustomerExists()
    {
        // Arrange
        var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var request = new GetCustomerRequest(customerId);

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.CustomerId.Should().Be(customerId);
        response.Name.Should().Be("John Doe");
        response.Email.Should().Be("john.doe@example.com");
        response.Phone.Should().Be("555-0100");
    }

    [Fact]
    public async Task Handle_ShouldIncludeOrders_WhenCustomerHasOrders()
    {
        // Arrange
        var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var request = new GetCustomerRequest(customerId);

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Orders.Should().NotBeNull();
        response.Orders.Should().HaveCount(1);
        response.Orders.First().Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyOrders_WhenCustomerHasNoOrders()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = "No Orders Customer",
            Email = "no.orders@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var request = new GetCustomerRequest(customer.CustomerId);

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Orders.Should().NotBeNull();
        response.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenCustomerDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new GetCustomerRequest(nonExistentId);

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Customer with ID '{nonExistentId}' not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectOrderSummaries()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "test.customer@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var order1 = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = customer.CustomerId,
            OrderDate = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            TotalAmount = 100.00m,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var order2 = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = customer.CustomerId,
            OrderDate = new DateTime(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc),
            TotalAmount = 200.00m,
            Status = "Completed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Orders.AddRange(order1, order2);
        await _context.SaveChangesAsync();

        var request = new GetCustomerRequest(customer.CustomerId);

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Orders.Should().HaveCount(2);
        response.Orders.Should().Contain(o => o.OrderId == order1.OrderId && o.TotalAmount == 100.00m);
        response.Orders.Should().Contain(o => o.OrderId == order2.OrderId && o.TotalAmount == 200.00m);
    }
}
