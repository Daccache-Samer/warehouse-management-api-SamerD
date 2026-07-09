using MediatR;

namespace Warehouse.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(string ProductId) : IRequest<ProductDto?>;