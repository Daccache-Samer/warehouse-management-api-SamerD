using Warehouse.DomainWarehouse.Domain.Suppliers;
using System.Reflection;

namespace Warehouse.Api.UnitTests.TestUtilities.Builders;

public class SupplierBuilder
{
    private string _name = "Default Supplier";
    private const string Country = "USA";
    private const string ContactEmail = "supplier@default.com";
    private const string ContactPhone = "+1-555-0100";
    private string _id = Guid.NewGuid().ToString();
    private bool _isActive = true;

    public SupplierBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public SupplierBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public SupplierBuilder IsInactive()
    {
        _isActive = false;
        return this;
    }

    public Supplier Build()
    {
        var supplier = Supplier.Create(_name, Country, ContactEmail, ContactPhone);
        
        var idProperty = typeof(Supplier).GetProperty("SupplierId", BindingFlags.Public | BindingFlags.Instance);
        if (idProperty != null)
        {
            var backingField = typeof(Supplier).GetField($"<{idProperty.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (backingField != null)
            {
                backingField.SetValue(supplier, _id);
            }
        }

        if (!_isActive)
        {
            supplier.Deactivate();
        }

        return supplier;
    }
}
