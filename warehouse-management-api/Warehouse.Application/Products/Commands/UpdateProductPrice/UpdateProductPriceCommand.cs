using MediatR;
using Warehouse.Application.Products.ViewModels;

namespace Warehouse.Application.Products.Commands.UpdateProductPrice;

public record UpdateProductPriceCommand(string ProductId, decimal Price) : IRequest<ProductViewModel>;