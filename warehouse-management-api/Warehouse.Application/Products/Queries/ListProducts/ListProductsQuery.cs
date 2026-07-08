using MediatR;

namespace Warehouse.Application.Products.Queries.ListProducts;

public abstract record ListProductsQuery(bool OnlyAvailable = false) : IRequest<IReadOnlyList<ProductDto>>;