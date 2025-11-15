using MediatR;

namespace HOB.API.Orders.UpdateOrder;

public record UpdateOrderRequest(
    Guid OrderId,
    string Status
) : IRequest<UpdateOrderResponse>;
