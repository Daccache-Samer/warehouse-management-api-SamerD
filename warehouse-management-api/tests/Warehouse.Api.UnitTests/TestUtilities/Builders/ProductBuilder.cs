using Warehouse.DomainWarehouse.Domain.Products;
using System.Reflection;

namespace Warehouse.Api.UnitTests.TestUtilities.Builders;

public class ProductBuilder
{
    private string _name = "Default Product";
    private string _sku = "SKU-DEFAULT";
    private const string Description = "Default Description";
    private const decimal Price = 100m;
    private const int QuantityInStock = 10;
    private readonly DateTime _expiryDate = DateTime.UtcNow.AddYears(1);
    private string _id = Guid.NewGuid().ToString();
    private string? _supplierId;

    public ProductBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductBuilder WithSku(string sku)
    {
        _sku = sku;
        return this;
    }

    public ProductBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ProductBuilder WithSupplierId(string supplierId)
    {
        _supplierId = supplierId;
        return this;
    }

    public Product Build()
    {
        var product = Product.Create(_name, _sku, Description, Price, QuantityInStock, _expiryDate);
        
        var idProperty = typeof(Product).GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProperty != null)
        {
            var backingField = typeof(Product).GetField(
                $"<{idProperty.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            if (backingField != null)
            {
                backingField.SetValue(product, _id);
            }
        }

        if (_supplierId == null) return product;
        {
            var supplierIdProperty = typeof(Product).GetProperty(
                "SupplierId", BindingFlags.Public | BindingFlags.Instance);
            if (supplierIdProperty == null) return product;
            var backingField = typeof(Product).GetField($"<{supplierIdProperty.Name}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (backingField != null)
            {
                backingField.SetValue(product, _supplierId);
            }
        }

        return product;
    }
}
