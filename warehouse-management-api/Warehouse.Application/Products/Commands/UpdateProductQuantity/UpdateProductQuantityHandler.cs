using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.UpdateProductQuantity;

public class UpdateProductQuantityHandler(IProductRepository productRepository)
    : IRequestHandler<UpdateProductQuantityCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductQuantityCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        product.UpdateQuantity(request.QuantityInStock);

        await productRepository.UpdateAsync(product, cancellationToken);

        return ProductDto.FromDomain(product);
    }
}