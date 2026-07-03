using warehouse_management_api.Contracts;
using warehouse_management_api.Models;

namespace warehouse_management_api.Services;

public interface ISupplierService
{
    IEnumerable<Supplier> GetAll();
    Supplier? GetById(string id);
    Supplier Create(CreateSupplierRequest request);
    bool Deactivate(string id);
    bool Exists(string id);
    bool IsActive(string id);
}