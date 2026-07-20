using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.ArchiveProduct;

public class ArchiveProductHandler(
    IProductRepository productRepository,ILogger<ArchiveProductHandler> logger,IDistributedCache cache) 
    : IRequestHandler<ArchiveProductCommand>
{
    public async Task Handle(ArchiveProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        product.Archive();

        await productRepository.UpdateAsync(product, cancellationToken);
        await cache.RemoveAsync($"GetProductByIdQuery-{product.Id}", cancellationToken);
        await cache.RemoveAsync("ListProductsHandler_ListProductsQuery", cancellationToken);

        logger.LogInformation("Product archived: {ProductId} {Sku}", product.Id, product.SKU);
    }
}