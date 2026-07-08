using MediatR;

namespace Warehouse.Application.Products.Queries.SearchProducts;

public record SearchProductsQuery(string? Name, string? Supplier) : IRequest<IReadOnlyList<ProductDto>>;