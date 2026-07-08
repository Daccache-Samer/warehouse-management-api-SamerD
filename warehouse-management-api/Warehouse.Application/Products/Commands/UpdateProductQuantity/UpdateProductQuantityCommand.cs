using MediatR;

namespace Warehouse.Application.Products.Commands.UpdateProductQuantity;

public abstract record UpdateProductQuantityCommand(string ProductId, int QuantityInStock) : IRequest<ProductDto>;