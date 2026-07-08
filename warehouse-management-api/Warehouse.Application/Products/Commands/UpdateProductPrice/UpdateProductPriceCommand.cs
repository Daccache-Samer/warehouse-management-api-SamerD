using MediatR;

namespace Warehouse.Application.Products.Commands.UpdateProductPrice;

public record UpdateProductPriceCommand(string ProductId, decimal Price) : IRequest<ProductDto>;