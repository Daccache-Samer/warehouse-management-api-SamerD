using MediatR;

namespace Warehouse.Application.Products.Queries.SearchProducts;

public abstract record SearchProductsQuery(string? Name, string? Supplier) : IRequest<IReadOnlyList<ProductDto>>;