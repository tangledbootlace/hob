using MediatR;

namespace HOB.API.Orders.GetOrder;

public record GetOrderRequest(Guid OrderId) : IRequest<GetOrderResponse>;
