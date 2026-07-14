using MediatR;
using Warehouse.Application.Products.ViewModels;

namespace Warehouse.Application.Products.Queries.ListProducts;

public record ListProductsQuery(bool OnlyAvailable = false) : IRequest<IReadOnlyList<ProductViewModel>>;