using MediatR;

namespace Warehouse.Application.Products.Queries.GetProductById;

public abstract record GetProductByIdQuery(string ProductId) : IRequest<ProductDto?>;