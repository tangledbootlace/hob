using FluentAssertions;
using HOB.API.Customers.CreateCustomer;
using HOB.Data;
using HOB.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace HOB.API.Tests.Customers;

public class CreateCustomerRequestHandlerTests : IDisposable
{
    private readonly HobDbContext _context;
    private readonly Mock<ILogger<CreateCustomerRequestHandler>> _loggerMock;
    private readonly CreateCustomerRequestHandler _handler;

    public CreateCustomerRequestHandlerTests()
    {
        var options = new DbContextOptionsBuilder<HobDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new HobDbContext(options);
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<CreateCustomerRequestHandler>>();
        _handler = new CreateCustomerRequestHandler(_context, _loggerMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldCreateCustomer_WhenValidRequest()
    {
        // Arrange
        var request = new CreateCustomerRequest(
            "John Smith",
            "john.smith@example.com",
            "555-1234"
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Name.Should().Be("John Smith");
        response.Email.Should().Be("john.smith@example.com");
        response.Phone.Should().Be("555-1234");
        response.CustomerId.Should().NotBe(Guid.Empty);
        response.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        response.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ShouldSaveCustomerToDatabase_WhenValidRequest()
    {
        // Arrange
        var request = new CreateCustomerRequest(
            "Jane Doe",
            "jane.doe@example.com",
            null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        var customer = await _context.Customers.FindAsync(response.CustomerId);
        customer.Should().NotBeNull();
        customer!.Name.Should().Be("Jane Doe");
        customer.Email.Should().Be("jane.doe@example.com");
        customer.Phone.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingCustomer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            Name = "Existing Customer",
            Email = "duplicate@example.com",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(existingCustomer);
        await _context.SaveChangesAsync();

        var request = new CreateCustomerRequest(
            "New Customer",
            "duplicate@example.com",
            "555-5555"
        );

        // Act
        var act = async () => await _handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Customer with email 'duplicate@example.com' already exists");
    }

    [Fact]
    public async Task Handle_ShouldAllowNullPhone()
    {
        // Arrange
        var request = new CreateCustomerRequest(
            "Test User",
            "test.user@example.com",
            null
        );

        // Act
        var response = await _handler.Handle(request, CancellationToken.None);

        // Assert
        response.Phone.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenCustomerCreated()
    {
        // Arrange
        var request = new CreateCustomerRequest(
            "Log Test",
            "log.test@example.com",
            "555-9999"
        );

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created customer")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
