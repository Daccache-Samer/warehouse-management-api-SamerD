using warehouse_management_api.Contracts;
using warehouse_management_api.Data;
using warehouse_management_api.Models;

namespace warehouse_management_api.Services;

public class SupplierService : ISupplierService
{
    public IEnumerable<Supplier> GetAll()
    {
        return FakeSuppliers.Suppliers;
    }

    public Supplier? GetById(string id)
    {
        return FakeSuppliers.Suppliers.FirstOrDefault(s => s.Id == id);
    }

    public Supplier Create(CreateSupplierRequest request)
    {
        var supplier = new Supplier
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Country = request.Country,
            ContactEmail = request.ContactEmail,
            PhoneNumber = request.PhoneNumber,
            IsActive = true
        };

        FakeSuppliers.Suppliers.Add(supplier);
        return supplier;
    }

    public bool Deactivate(string id)
    {
        var supplier = GetById(id);
        if (supplier is null)
        {
            return false;
        }

        supplier.IsActive = false;
        return true;
    }

    public bool Exists(string id)
    {
        return FakeSuppliers.Suppliers.Any(s => s.Id == id);
    }

    public bool IsActive(string id)
    {
        return FakeSuppliers.Suppliers.Any(s => s.Id == id && s.IsActive);
    }
}