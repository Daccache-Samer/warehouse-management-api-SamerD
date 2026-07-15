namespace Warehouse.DomainWarehouse.Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<int> CountLowStockAsync(int lowStockThreshold, CancellationToken ct = default);
    Task<decimal> GetTotalInventoryValueAsync(CancellationToken ct = default);
}