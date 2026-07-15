using Warehouse.DomainWarehouse.Domain.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Domain.Tests.Products;

public class StockAdjustmentRuleTests
{
    private static Product CreateValidProduct() =>
        Product.Create("Laptop", "SKU-001", "A laptop", 100m, 10, DateTime.UtcNow.AddYears(1));

    [Fact]
    public void AdjustQuantity_WithPositiveDelta_IncreasesQuantity()
    {
        var product = CreateValidProduct();

        product.AdjustQuantity(5);

        Assert.Equal(15, product.QuantityInStock);
    }

    [Fact]
    public void AdjustQuantity_WithNegativeDeltaWithinStock_DecreasesQuantity()
    {
        var product = CreateValidProduct();

        product.AdjustQuantity(-4);

        Assert.Equal(6, product.QuantityInStock);
    }

    [Fact]
    public void AdjustQuantity_WithNegativeDeltaExceedingStock_Throws()
    {
        var product = CreateValidProduct();

        Assert.Throws<DomainException>(() => product.AdjustQuantity(-11));
    }

    [Fact]
    public void AdjustQuantity_OnArchivedProduct_Throws()
    {
        var product = CreateValidProduct();
        product.Archive();

        Assert.Throws<DomainException>(() => product.AdjustQuantity(1));
    }
}