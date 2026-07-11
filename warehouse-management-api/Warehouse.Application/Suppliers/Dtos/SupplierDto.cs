using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers;

public record SupplierDto(
    string Id,
    string Name,
    string Country,
    string ContactEmail,
    string PhoneNumber,
    bool IsActive)
{
    public static SupplierDto FromDomain(Supplier supplier) => new(
        supplier.SupplierId,
        supplier.Name,
        supplier.Country,
        supplier.ContactEmail,
        supplier.PhoneNumber,
        supplier.IsActive);
}