using MediatR;

namespace HOB.API.Orders.DeleteOrder;

public record DeleteOrderRequest(Guid OrderId) : IRequest;
