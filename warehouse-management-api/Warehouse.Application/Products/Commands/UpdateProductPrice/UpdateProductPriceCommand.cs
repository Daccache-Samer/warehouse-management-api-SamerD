using MediatR;

namespace Warehouse.Application.Products.Commands.UpdateProductPrice;

public abstract record UpdateProductPriceCommand(string ProductId, decimal Price) : IRequest<ProductDto>;