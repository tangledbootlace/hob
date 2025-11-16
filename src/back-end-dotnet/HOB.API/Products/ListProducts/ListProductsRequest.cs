using MediatR;

namespace HOB.API.Products.ListProducts;

public record ListProductsRequest(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Category = null,
    bool? LowStock = null,
    bool? ActiveOnly = true
) : IRequest<ListProductsResponse>;
