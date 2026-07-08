namespace Warehouse.DomainWarehouse.Domain.Products;

public interface IFileStorage
{
    Task<string> SaveAsync(string productId, Stream content, string fileName, CancellationToken ct = default);
}