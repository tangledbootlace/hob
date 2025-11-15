namespace HOB.API.Customers.CreateCustomer;

public record CreateCustomerResponse(
    Guid CustomerId,
    string Name,
    string Email,
    string? Phone,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
