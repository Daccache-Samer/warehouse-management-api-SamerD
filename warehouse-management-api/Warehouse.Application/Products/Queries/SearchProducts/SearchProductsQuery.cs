using MediatR;
using Warehouse.Application.Products.ViewModels;

namespace Warehouse.Application.Products.Queries.SearchProducts;

public record SearchProductsQuery(string? Name, string? Supplier) : IRequest<IReadOnlyList<ProductViewModel>>;