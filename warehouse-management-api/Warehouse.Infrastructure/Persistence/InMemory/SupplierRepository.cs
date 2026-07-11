using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Infrastructure.Persistence.InMemory;

public class SupplierRepository : ISupplierRepository
{
    private readonly List<Supplier> _suppliers = SeedData();

    public Task<Supplier?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var supplier = _suppliers.FirstOrDefault(s => s.SupplierId == id);
        return Task.FromResult(supplier);
    }

    public Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Supplier> result = _suppliers.AsReadOnly();
        return Task.FromResult(result);
    }

    public Task AddAsync(Supplier supplier, CancellationToken ct = default)
    {
        _suppliers.Add(supplier);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Supplier supplier, CancellationToken ct = default)
    {
        // Same in-memory rationale as ProductRepository.UpdateAsync above.
        return Task.CompletedTask;
    }

    private static List<Supplier> SeedData()
    {
        return
        [
            Supplier.Create("TechSupply Co", "USA", "contact@techsupply.com", "+1-555-0101"),
            Supplier.Create("OfficeGear Ltd", "UK", "sales@officegear.co.uk", "+44-20-7946-0102"),
            Supplier.Create("DisplayTech", "South Korea", "info@displaytech.kr", "+82-2-555-0103")
        ];
    }
}