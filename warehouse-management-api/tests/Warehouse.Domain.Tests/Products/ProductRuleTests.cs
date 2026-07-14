using Warehouse.DomainWarehouse.Domain.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;
using Xunit;

namespace Warehouse.Domain.Tests.Products;

public class ProductRuleTests
{
    private static Product CreateValidProduct() =>
        Product.Create("Laptop", "SKU-001", "A laptop", 100m, 10, DateTime.UtcNow.AddYears(1));

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Create_WithZeroOrNegativePrice_Throws(decimal invalidPrice)
    {
        Assert.Throws<DomainException>(() =>
            Product.Create("Laptop", "SKU-001", "desc", invalidPrice, 10, DateTime.UtcNow));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void UpdatePrice_WithZeroOrNegativeValue_Throws(decimal invalidPrice)
    {
        var product = CreateValidProduct();

        Assert.Throws<DomainException>(() => product.UpdatePrice(invalidPrice));
    }

    [Fact]
    public void Create_WithNegativeQuantity_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Product.Create("Laptop", "SKU-001", "desc", 100m, -1, DateTime.UtcNow));
    }

    [Fact]
    public void UpdateQuantity_WithNegativeValue_Throws()
    {
        var product = CreateValidProduct();

        Assert.Throws<DomainException>(() => product.UpdateQuantity(-1));
    }

    [Fact]
    public void UpdatePrice_OnArchivedProduct_Throws()
    {
        var product = CreateValidProduct();
        product.Archive();

        Assert.Throws<DomainException>(() => product.UpdatePrice(200m));
    }

    [Fact]
    public void UpdateQuantity_OnArchivedProduct_Throws()
    {
        var product = CreateValidProduct();
        product.Archive();

        Assert.Throws<DomainException>(() => product.UpdateQuantity(5));
    }

    [Fact]
    public void AssignSupplier_WithInactiveSupplier_Throws()
    {
        var product = CreateValidProduct();
        var supplier = Supplier.Create("Acme Co", "USA", "contact@acme.com", "+1-555-0100");
        supplier.Deactivate();

        Assert.Throws<DomainException>(() => product.AssignSupplier(supplier));
    }

    [Fact]
    public void AssignSupplier_WithActiveSupplier_Succeeds()
    {
        var product = CreateValidProduct();
        var supplier = Supplier.Create("Acme Co", "USA", "contact@acme.com", "+1-555-0100");

        product.AssignSupplier(supplier);

        Assert.Equal(supplier.SupplierId, product.SupplierId);
    }
}