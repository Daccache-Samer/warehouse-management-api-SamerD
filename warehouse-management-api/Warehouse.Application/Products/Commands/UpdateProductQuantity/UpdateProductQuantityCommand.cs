using MediatR;

namespace Warehouse.Application.Products.Commands.UpdateProductQuantity;

public record UpdateProductQuantityCommand(string ProductId, int QuantityInStock) : IRequest<ProductDto>;