using MediatR;
using Microsoft.Extensions.Logging;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.ArchiveProduct;

public class ArchiveProductHandler(
    IProductRepository productRepository,
    IFileStorage fileStorage,
    ILogger<ArchiveProductHandler> logger)
    : IRequestHandler<ArchiveProductCommand>
{
    public async Task Handle(ArchiveProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        // Detach images first so the removal is captured by EF's change tracker
        // on the same tracked instance that UpdateAsync will persist.
        var removedImages = product.ClearImages();
        product.Archive();

        // Single DB transaction: archives the product and deletes the
        // now-orphaned ProductImage rows (required FK relationship ->
        // EF marks them Deleted when removed from the collection).
        await productRepository.UpdateAsync(product, cancellationToken);

        // Blob cleanup happens after the DB commit succeeds, and failures
        // here are logged but non-fatal: an orphaned blob is an acceptable
        // outcome, a product archived-in-DB-but-500-to-the-client is not.
        foreach (var image in removedImages)
        {
            try
            {
                await fileStorage.DeleteAsync(image.ObjectKey, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to delete blob {ObjectKey} for archived product {ProductId}; DB record was removed regardless.",
                    image.ObjectKey, product.Id);
            }
        }

        logger.LogInformation(
            "Product archived: {ProductId} {Sku}, {ImageCount} associated image(s) cascaded for deletion.",
            product.Id, product.SKU, removedImages.Count);
    }
}