using Warehouse.DomainWarehouse.Domain.Common;

namespace Warehouse.DomainWarehouse.Domain.Suppliers;

public interface ISupplierRepository :  IRepository<Supplier>
{
    Task<int> CountActiveSuppliersAsync(CancellationToken ct = default);
}