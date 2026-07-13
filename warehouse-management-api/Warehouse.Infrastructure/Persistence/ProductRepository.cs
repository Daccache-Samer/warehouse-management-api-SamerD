using Microsoft.EntityFrameworkCore;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Infrastructure.Persistence;

public class ProductRepository(WarehouseDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await context.Products.Include("_images").FirstOrDefaultAsync(p => p.Id == id, ct);
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
}