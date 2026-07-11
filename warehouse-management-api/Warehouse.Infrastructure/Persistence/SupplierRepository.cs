using Microsoft.EntityFrameworkCore;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Infrastructure.Persistence;

public class SupplierRepository(WarehouseDbContext context) : ISupplierRepository
{
    public async Task<Supplier?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        return await context.Suppliers.FirstOrDefaultAsync(s => s.SupplierId == id, ct);
    }

    public async Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Suppliers.AsNoTracking().ToListAsync(ct);
    }

    public async Task AddAsync(Supplier supplier, CancellationToken ct = default)
    {
        await context.Suppliers.AddAsync(supplier, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Supplier supplier, CancellationToken ct = default)
    {
        await context.SaveChangesAsync(ct);
    }
}