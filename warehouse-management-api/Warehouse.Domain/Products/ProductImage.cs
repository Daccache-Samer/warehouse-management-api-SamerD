using Warehouse.DomainWarehouse.Domain.Exceptions;
namespace Warehouse.DomainWarehouse.Domain.Products;

public class ProductImage
{
    public string ProductId { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ObjectKey { get; private set; } = string.Empty;

    private ProductImage() { }

    public static ProductImage Create(string productId, string fileName, string objectKey)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new DomainException("ProductId is required.");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("FileName is required.");

        if (string.IsNullOrWhiteSpace(objectKey))
            throw new DomainException("objectKey is required.");

        return new ProductImage
        {
            ProductId = productId,
            FileName = fileName,
            ObjectKey = objectKey
        };
    }
}