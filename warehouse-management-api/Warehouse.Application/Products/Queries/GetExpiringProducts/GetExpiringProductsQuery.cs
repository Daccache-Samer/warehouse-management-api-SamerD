using MediatR;
using Warehouse.Application.Products.ViewModels;

namespace Warehouse.Application.Products.Queries.GetExpiringProducts;

public record GetExpiringProductsQuery(int WithinDays) : IRequest<IReadOnlyList<ExpiringProductViewModel>>;