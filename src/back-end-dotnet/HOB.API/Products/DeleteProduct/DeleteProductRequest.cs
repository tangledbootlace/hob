using MediatR;

namespace HOB.API.Products.DeleteProduct;

public record DeleteProductRequest(Guid ProductId) : IRequest;
