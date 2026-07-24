using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Queries.DownloadProductImage;

public class DownloadProductImageHandler(IProductRepository productRepository, IFileStorage fileStorage)
    : IRequestHandler<DownloadProductImageQuery, ProductImageDownloadResult>
{
    public async Task<ProductImageDownloadResult> Handle(DownloadProductImageQuery request, CancellationToken ct)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, ct)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        var image = product.Images.FirstOrDefault(i => i.FileName == request.FileName)
                    ?? throw new NotFoundException($"Image '{request.FileName}' was not found on this product.");

        var (content, contentType) = await fileStorage.DownloadAsync(image.ObjectKey, ct); // FilePath now holds the object key
        return new ProductImageDownloadResult(content, contentType, image.FileName);
    }
}