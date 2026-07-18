using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.UpdateProductQuantity;

public class UpdateProductQuantityHandler(IProductRepository productRepository,IMapper mapper,IDistributedCache cache)
    : IRequestHandler<UpdateProductQuantityCommand, ProductViewModel>
{
    public async Task<ProductViewModel> Handle(UpdateProductQuantityCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        product.UpdateQuantity(request.QuantityInStock);

        await productRepository.UpdateAsync(product, cancellationToken);
        await cache.RemoveAsync($"GetProductByIdQuery-{product.Id}", cancellationToken);
        await cache.RemoveAsync("ListProductsHandler_ListProductsQuery", cancellationToken);

        return mapper.Map<ProductViewModel>(product);
    }
}