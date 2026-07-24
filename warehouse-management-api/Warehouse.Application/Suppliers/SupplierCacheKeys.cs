using Warehouse.Application.Suppliers.Queries.GetSuppliersById;
using Warehouse.Application.Suppliers.Queries.ListSuppliers;

namespace Warehouse.Application.Suppliers;

public static class SupplierCacheKeys
{
    public static string ById(string id) => $"{nameof(GetSupplierByIdQuery)}--{id}";
    public const string List = $"{nameof(ListSuppliersQuery)}";
}