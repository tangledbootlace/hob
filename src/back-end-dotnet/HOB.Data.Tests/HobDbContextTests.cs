using FluentAssertions;
using HOB.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HOB.Data.Tests;

public class HobDbContextTests : IDisposable
{
    private readonly HobDbContext _context;

    public HobDbContextTests()
    {
        var options = new DbContextOptionsBuilder<HobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HobDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public void HobDbContext_ShouldHaveCustomersDbSet()
    {
        // Assert
        _context.Customers.Should().NotBeNull();
    }

    [Fact]
    public void HobDbContext_ShouldHaveOrdersDbSet()
    {
        // Assert
        _context.Orders.Should().NotBeNull();
    }

    [Fact]
    public void HobDbContext_ShouldHaveSalesDbSet()
    {
        // Assert
        _context.Sales.Should().NotBeNull();
    }

    [Fact]
    public async Task HobDbContext_ShouldSeedCustomers()
    {
        // Act
        var customers = await _context.Customers.ToListAsync();

        // Assert
        customers.Should().HaveCount(2);
        customers.Should().Contain(c => c.Email == "john.doe@example.com");
        customers.Should().Contain(c => c.Email == "jane.smith@example.com");
    }

    [Fact]
    public async Task HobDbContext_ShouldSeedOrders()
    {
        // Act
        var orders = await _context.Orders.ToListAsync();

        // Assert
        orders.Should().HaveCount(2);
        orders.Should().Contain(o => o.Status == "Completed");
        orders.Should().Contain(o => o.Status == "Pending");
    }

    [Fact]
    public async Task HobDbContext_ShouldSeedSales()
    {
        // Act
        var sales = await _context.Sales.ToListAsync();

        // Assert
        sales.Should().HaveCount(3);
        sales.Should().Contain(s => s.ProductName == "Widget A");
        sales.Should().Contain(s => s.ProductName == "Widget B");
        sales.Should().Contain(s => s.ProductName == "Widget C");
    }

    [Fact]
    public void Customer_ShouldHaveUniqueEmailIndexConfigured()
    {
        // Arrange
        var entityType = _context.Model.FindEntityType(typeof(Customer));

        // Act
        var emailIndex = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "Email"));

        // Assert
        emailIndex.Should().NotBeNull("a unique index should be configured on the Email property");
        emailIndex!.IsUnique.Should().BeTrue("the Email index should be unique");
    }

    [Fact]
    public async Task Order_ShouldCascadeDeleteToSales()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = "Test Customer",
            Email = "cascade@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = customer.CustomerId,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 100.00m,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var sale = new Sale
        {
            SaleId = Guid.NewGuid(),
            OrderId = order.OrderId,
            ProductName = "Test Product",
            Quantity = 1,
            UnitPrice = 100.00m,
            TotalPrice = 100.00m,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        _context.Orders.Add(order);
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        // Act
        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        // Assert
        var salesCount = await _context.Sales.CountAsync(s => s.OrderId == order.OrderId);
        salesCount.Should().Be(0);
    }

    [Fact]
    public async Task Customer_ShouldLoadOrdersNavigationProperty()
    {
        // Arrange
        var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var customer = await _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        // Assert
        customer.Should().NotBeNull();
        customer!.Orders.Should().HaveCount(1);
    }

    [Fact]
    public async Task Order_ShouldLoadSalesNavigationProperty()
    {
        // Arrange
        var orderId = Guid.Parse("10000000-1000-1000-1000-100000000001");

        // Act
        var order = await _context.Orders
            .Include(o => o.Sales)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        // Assert
        order.Should().NotBeNull();
        order!.Sales.Should().HaveCount(2);
    }

    [Fact]
    public async Task HobDbContext_ShouldAddCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = "New Customer",
            Email = "new.customer@example.com",
            Phone = "555-9999",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Assert
        var savedCustomer = await _context.Customers.FindAsync(customer.CustomerId);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Email.Should().Be("new.customer@example.com");
    }

    [Fact]
    public async Task HobDbContext_ShouldUpdateCustomer()
    {
        // Arrange
        var customerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var customer = await _context.Customers.FindAsync(customerId);

        // Act
        customer!.Name = "Updated Name";
        customer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Assert
        var updatedCustomer = await _context.Customers.FindAsync(customerId);
        updatedCustomer!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task HobDbContext_ShouldDeleteCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = "To Delete",
            Email = "delete.me@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        // Act
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        // Assert
        var deletedCustomer = await _context.Customers.FindAsync(customer.CustomerId);
        deletedCustomer.Should().BeNull();
    }
}
