using MediatR;
using Warehouse.Application.Products.ViewModels;

namespace Warehouse.Application.Products.Commands.UpdateProductQuantity;

public record UpdateProductQuantityCommand(string ProductId, int QuantityInStock) : IRequest<ProductViewModel>;