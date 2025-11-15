namespace HOB.API.Customers.ListCustomers;

public record ListCustomersResponse(
    List<CustomerItem> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);

public record CustomerItem(
    Guid CustomerId,
    string Name,
    string Email,
    string? Phone,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
