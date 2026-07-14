using MediatR;
using Warehouse.Application.Products.ViewModels;

namespace Warehouse.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(string ProductId) : IRequest<ProductViewModel?>;