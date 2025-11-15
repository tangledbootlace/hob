namespace HOB.API.Customers.GetCustomer;

public record GetCustomerResponse(
    Guid CustomerId,
    string Name,
    string Email,
    string? Phone,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<OrderSummary> Orders
);

public record OrderSummary(
    Guid OrderId,
    DateTime OrderDate,
    decimal TotalAmount,
    string Status
);
