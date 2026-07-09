using NSubstitute;
using Warehouse.Application.Products.Commands.CreateProduct;
using Warehouse.DomainWarehouse.Domain.Products;
using Xunit;

namespace Warehouse.Application.Tests.Products;

public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_WithValidRequest_CallsRepositoryAddAsync()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetBySkuAsync("SKU-001", Arg.Any<CancellationToken>())
            .Returns((Product?)null); // no existing product with this SKU

        var handler = new CreateProductHandler(repository);
        var command = new CreateProductCommand(
            "Laptop", "SKU-001", "A laptop", 999m, 5, DateTime.UtcNow.AddYears(1));

        var result = await handler.Handle(command, CancellationToken.None);

        await repository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.SKU == "SKU-001" && p.Name == "Laptop"),
            Arg.Any<CancellationToken>());

        Assert.Equal("Laptop", result.Name);
        Assert.Equal("SKU-001", result.SKU);
    }

    [Fact]
    public async Task Handle_WithDuplicateSku_ThrowsAndDoesNotCallAddAsync()
    {
        var existingProduct = Product.Create("Old Laptop", "SKU-001", "desc", 100m, 1, DateTime.UtcNow);

        var repository = Substitute.For<IProductRepository>();
        repository.GetBySkuAsync("SKU-001", Arg.Any<CancellationToken>())
            .Returns(existingProduct);

        var handler = new CreateProductHandler(repository);
        var command = new CreateProductCommand(
            "New Laptop", "SKU-001", "desc", 200m, 5, DateTime.UtcNow);

        await Assert.ThrowsAsync<Warehouse.Application.Exceptions.ConflictException>(
            () => handler.Handle(command, CancellationToken.None));

        await repository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }
}