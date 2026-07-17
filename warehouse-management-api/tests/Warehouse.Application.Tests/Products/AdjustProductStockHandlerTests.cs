using NSubstitute;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.Commands.AdjustProductStock;
using Warehouse.DomainWarehouse.Domain.Products;
using Xunit;

namespace Warehouse.Application.Tests.Products;

public class AdjustProductStockHandlerTests
{
    [Fact]
    public async Task Handle_WithUnknownProductId_ThrowsNotFoundAndDoesNotCallUpdate()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync("missing-id", Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var handler = new AdjustProductStockHandler(repository, MapperFactory.Create());
        var command = new AdjustProductStockCommand("missing-id", StockAdjustmentType.Increase, 5, null);

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));

        await repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithIncrease_CallsUpdateWithIncreasedQuantity()
    {
        var product = Product.Create("Laptop", "SKU-001", "desc", 100m, 10, DateTime.UtcNow.AddYears(1));

        var repository = Substitute.For<IProductRepository>();
        repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var handler = new AdjustProductStockHandler(repository, MapperFactory.Create());
        var command = new AdjustProductStockCommand(product.Id, StockAdjustmentType.Increase, 5, null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(15, result.QuantityInStock);
        await repository.Received(1).UpdateAsync(
            Arg.Is<Product>(p => p.QuantityInStock == 15), Arg.Any<CancellationToken>());
    }
}