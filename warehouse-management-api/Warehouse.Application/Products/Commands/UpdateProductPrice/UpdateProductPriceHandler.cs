using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.UpdateProductPrice;

public class UpdateProductPriceHandler(IProductRepository productRepository)
    : IRequestHandler<UpdateProductPriceCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        product.UpdatePrice(request.Price);

        await productRepository.UpdateAsync(product, cancellationToken);

        return ProductDto.FromDomain(product);
    }
}