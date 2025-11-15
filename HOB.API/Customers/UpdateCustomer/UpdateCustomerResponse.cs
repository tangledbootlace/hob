namespace HOB.API.Customers.UpdateCustomer;

public record UpdateCustomerResponse(
    Guid CustomerId,
    string Name,
    string Email,
    string? Phone,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
