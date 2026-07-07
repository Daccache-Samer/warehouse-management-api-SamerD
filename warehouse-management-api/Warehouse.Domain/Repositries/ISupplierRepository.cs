using Warehouse.DomainWarehouse.Domain.Models;

namespace Warehouse.DomainWarehouse.Domain.Repositries;

public interface ISupplierRepository
{
    Task<IEnumerable<Supplier>> GetAllAsync();
    Task<Supplier?> GetByIdAsync(string id);
    Task AddAsync(Supplier supplier);
    Task UpdateAsync(Supplier supplier);
    Task<bool> ExistsAsync(string id);
}