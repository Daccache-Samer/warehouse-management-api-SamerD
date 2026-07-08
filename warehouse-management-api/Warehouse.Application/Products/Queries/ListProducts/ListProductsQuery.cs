using MediatR;

namespace Warehouse.Application.Products.Queries.ListProducts;

public record ListProductsQuery(bool OnlyAvailable = false) : IRequest<IReadOnlyList<ProductDto>>;