using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application.Products.Commands.ArchiveProduct;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Api.UnitTests.ProductService;

public class ArchiveProduct
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly ArchiveProductHandler _handler;

    public ArchiveProduct()
    {
        var loggerMock = new Mock<ILogger<ArchiveProductHandler>>();

        _handler = new ArchiveProductHandler(
            _productRepositoryMock.Object,
            loggerMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ValidProduct_MarksArchivedOnlyAndDoesNotDelete()
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build();
        var command = new ArchiveProductCommand(product.Id);

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(
                product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        Product? capturedProduct = null;
        _productRepositoryMock.Setup(repo => repo.UpdateAsync(
                It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, ct) => capturedProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct.IsArchived.Should().BeTrue();
        
        _productRepositoryMock.Verify(repo => repo.UpdateAsync(
            It.Is<Product>(p => p.IsArchived), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(cache => cache.RemoveAsync(
            $"GetProductByIdQuery-{product.Id}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
