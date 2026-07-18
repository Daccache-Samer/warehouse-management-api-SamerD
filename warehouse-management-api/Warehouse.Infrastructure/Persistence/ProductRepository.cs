using Microsoft.EntityFrameworkCore;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Infrastructure.Persistence;

public class ProductRepository(WarehouseDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await context.Products.Include(p=>p.Images).FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
    {
        return await context.Products.AsNoTracking().FirstOrDefaultAsync(p =>
            EF.Functions.ILike(p.SKU, sku), ct);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Products.AsNoTracking().ToListAsync(ct);
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await context.Products.AddAsync(product, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        context.Update(product);
        await context.SaveChangesAsync(ct);
    }
    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await context.Products.CountAsync(p => !p.IsArchived, ct);
    }

    public async Task<int> CountLowStockAsync(int threshold, CancellationToken ct = default)
    {
        return await context.Products.CountAsync(p => !p.IsArchived && p.QuantityInStock <= threshold, ct);
    }
    public async Task<decimal> GetTotalInventoryValueAsync(CancellationToken ct = default)
    {
        return await context.Products.Where(p => !p.IsArchived)
            .SumAsync(p => p.Price * p.QuantityInStock, ct);
    }
    public async Task<IReadOnlyList<Product>> GetExpiringProductsAsync(int withinDays, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(withinDays);
        return await context.Products
            .AsNoTracking()
            .Where(p => !p.IsArchived && p.ExpiryDate <= cutoff)
            .ToListAsync(ct);
    }
}