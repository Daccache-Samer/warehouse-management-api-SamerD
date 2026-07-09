using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products;

public record ProductDto(
    string Id,
    string Name,
    string SKU,
    string Description,
    decimal Price,
    int QuantityInStock,
    string? SupplierId,
    DateTime ExpiryDate,
    bool IsArchived,
    DateTime CreatedAt,
    DateTime LastUpdatedAt)
{
    public static ProductDto FromDomain(Product product) => new(
        product.Id,
        product.Name,
        product.SKU,
        product.Description,
        product.Price,
        product.QuantityInStock,
        product.SupplierId,
        product.ExpiryDate,
        product.IsArchived,
        product.CreatedAt,
        product.LastUpdatedAt);
}