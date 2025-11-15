namespace HOB.API.Orders.UpdateOrder;

public record UpdateOrderResponse(
    Guid OrderId,
    Guid CustomerId,
    DateTime OrderDate,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
