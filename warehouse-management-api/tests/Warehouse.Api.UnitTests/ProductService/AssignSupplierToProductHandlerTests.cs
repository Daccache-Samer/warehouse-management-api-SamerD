using AutoMapper;
using FluentAssertions;
using Moq;
using Warehouse.Application.Products.Commands.AssignSupplierToProduct;
using Warehouse.DomainWarehouse.Domain.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Api.UnitTests.ProductService;

public class AssignSupplierToProductHandlerTests
{
    private readonly Mock<IProductRepository> _productRepository = new();
    private readonly Mock<ISupplierRepository> _supplierRepository = new();
    private readonly Mock<IMapper> _mapper = new();

    private AssignSupplierToProductHandler CreateSut() =>
        new(_productRepository.Object, _supplierRepository.Object, _mapper.Object);

    [Fact]
    public async Task Handle_ArchivedProduct_ThrowsDomainException_AndDoesNotPersist()
    {
        // Arrange
        var product = Product.Create("Widget", "SKU-001", "desc", 10m, 5, DateTime.UtcNow.AddDays(30));
        product.Archive();

        var supplier = Supplier.Create("Acme", "US", "a@acme.com", "1234567890");

        _productRepository
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _supplierRepository
            .Setup(r => r.GetByIdAsync(supplier.SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        var sut = CreateSut();
        var command = new AssignSupplierToProductCommand(product.Id, supplier.SupplierId);

        // Act
        Func<Task> act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Archived products cannot be updated.");

        _productRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);

        // Cache eviction assertion removed — invalidation now lives in
        // ProductCacheInvalidationBehavior, which never runs here anyway since
        // it only fires after a successful next() and the handler throws first.
    }
}