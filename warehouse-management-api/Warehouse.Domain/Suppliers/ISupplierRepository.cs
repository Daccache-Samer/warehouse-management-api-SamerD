namespace Warehouse.DomainWarehouse.Domain.Suppliers;

public interface ISupplierRepository
{
    Task<Supplier?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Supplier supplier, CancellationToken ct = default);
    Task UpdateAsync(Supplier supplier, CancellationToken ct = default);
}