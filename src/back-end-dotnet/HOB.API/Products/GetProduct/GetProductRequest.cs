using MediatR;

namespace HOB.API.Products.GetProduct;

public record GetProductRequest(Guid ProductId) : IRequest<GetProductResponse>;
