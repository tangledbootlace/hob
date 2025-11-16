using MediatR;

namespace HOB.API.Orders.CreateOrder;

public record CreateOrderRequest(
    Guid CustomerId,
    DateTime OrderDate,
    string Status,
    List<SaleItemRequest> SaleItems
) : IRequest<CreateOrderResponse>;

public record SaleItemRequest(
    Guid ProductId,
    int Quantity
);
