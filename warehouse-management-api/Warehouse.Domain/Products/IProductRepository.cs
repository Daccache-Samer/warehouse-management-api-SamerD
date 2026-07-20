using Warehouse.DomainWarehouse.Domain.Common;

namespace Warehouse.DomainWarehouse.Domain.Products;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<int> CountLowStockAsync(int lowStockThreshold, CancellationToken ct = default);
    Task<decimal> GetTotalInventoryValueAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetExpiringProductsAsync(int withinDays, CancellationToken ct = default);

}