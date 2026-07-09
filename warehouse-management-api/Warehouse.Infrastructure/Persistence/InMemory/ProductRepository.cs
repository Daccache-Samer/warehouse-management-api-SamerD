using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Infrastructure.Persistence.InMemory;

public class ProductRepository : IProductRepository
{
    private readonly List<Product> _products = SeedData();

    public Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        var product = _products.FirstOrDefault(p =>
            p.SKU.Equals(sku, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(product);
    }

    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Product> result = _products.AsReadOnly();
        return Task.FromResult(result);
    }

    public Task AddAsync(Product product, CancellationToken ct = default)
    {
        _products.Add(product);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        // In-memory: 'product' is already the same reference stored in _products,
        // so domain mutations (UpdatePrice, Archive, etc.) are already reflected.
        // Kept as a real call for interface parity with a future EF Core implementation,
        // where this would become: _db.Products.Update(product); await _db.SaveChangesAsync(ct);
        return Task.CompletedTask;
    }

    private static List<Product> SeedData()
    {
        return
        [
            Product.Create("laptop", "AA", "Laptop", 100m, 10, DateTime.UtcNow.AddYears(2)),
            Product.Create("GTA6", "BB", "GTA6", 80m, 100, DateTime.UtcNow.AddYears(2)),
            Product.Create("PS5", "CC", "PS5", 400m, 20, DateTime.UtcNow.AddYears(2)),
            Product.Create("RAM", "DD", "RAM", 10000m, 100, DateTime.UtcNow.AddYears(2)),
            Product.Create("pen", "EE", "pen", 10m, 1000, DateTime.UtcNow.AddYears(2)),
            Product.Create("Gaming Chairs", "FF", "Gaming Chairs", 200m, 100, DateTime.UtcNow.AddYears(2)),
            Product.Create("Bottle of water", "GG", "Bottle of water", 1m, 1000, DateTime.UtcNow.AddYears(2)),
            Product.Create("textbook", "HH", "textbook", 10m, 1000, DateTime.UtcNow.AddYears(2)),
            Product.Create("Phones", "II", "Phones", 400m, 500, DateTime.UtcNow.AddYears(2)),
            Product.Create("sneakers", "JJ", "sneakers", 300m, 1000, DateTime.UtcNow.AddYears(2))
        ];
    }
}