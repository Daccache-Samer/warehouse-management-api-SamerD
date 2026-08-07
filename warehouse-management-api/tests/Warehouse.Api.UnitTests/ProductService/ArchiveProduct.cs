using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application.Products.Commands.ArchiveProduct;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Api.UnitTests.ProductService;

public class ArchiveProduct
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IFileStorage> _fileStorageMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly ArchiveProductHandler _handler;

    public ArchiveProduct()
    {
        var loggerMock = new Mock<ILogger<ArchiveProductHandler>>();

        _handler = new ArchiveProductHandler(
            _productRepositoryMock.Object,
            _fileStorageMock.Object,
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
            .Callback<Product, CancellationToken>((p, _) => capturedProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct!.IsArchived.Should().BeTrue();

        _productRepositoryMock.Verify(repo => repo.UpdateAsync(
            It.Is<Product>(p => p.IsArchived), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(cache => cache.RemoveAsync(
            $"GetProductByIdQuery-{product.Id}", It.IsAny<CancellationToken>()), Times.Once);

        // This product had no images, so nothing should have hit blob storage.
        _fileStorageMock.Verify(fs => fs.DeleteAsync(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidProduct_ArchivedProductRemainsInRepositoryList()
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build();
        var command = new ArchiveProductCommand(product.Id);

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(
                product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _productRepositoryMock.Setup(repo =>
                repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        // Act
        await _handler.Handle(command, CancellationToken.None);
        var allProducts = await _productRepositoryMock.Object.GetAllAsync(CancellationToken.None);

        // Assert
        allProducts.Should().ContainSingle(p => p.Id == product.Id);
        allProducts.Single().IsArchived.Should().BeTrue();
        _productRepositoryMock.Verify(repo => repo.UpdateAsync(
            It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductWithImages_DeletesEachImageBlobAndClearsImageCollection()
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build();
        var image1 = ProductImage.Create(product.Id, "front.jpg", "products/p1/aaa-front.jpg");
        var image2 = ProductImage.Create(product.Id, "back.jpg", "products/p1/bbb-back.jpg");
        product.AddImage(image1);
        product.AddImage(image2);

        var command = new ArchiveProductCommand(product.Id);

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(
                product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.Images.Should().BeEmpty("ClearImages should detach all images from the aggregate");

        _fileStorageMock.Verify(fs => fs.DeleteAsync(
            "products/p1/aaa-front.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageMock.Verify(fs => fs.DeleteAsync(
            "products/p1/bbb-back.jpg", It.IsAny<CancellationToken>()), Times.Once);

        _productRepositoryMock.Verify(repo => repo.UpdateAsync(
            It.Is<Product>(p => p.IsArchived && p.Images.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BlobDeletionThrows_StillCompletesSuccessfully()
    {
        // Arrange: verifies the handler's documented non-fatal behavior —
        // a storage failure during cascade cleanup must not surface as an
        // unhandled exception or block the DB-level archive from completing.
        var product = new ProductBuilder().WithName("Laptop").Build();
        var image = ProductImage.Create(product.Id, "front.jpg", "products/p1/aaa-front.jpg");
        product.AddImage(image);

        var command = new ArchiveProductCommand(product.Id);

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(
                product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _fileStorageMock.Setup(fs => fs.DeleteAsync(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("storage unavailable"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _productRepositoryMock.Verify(repo => repo.UpdateAsync(
            It.Is<Product>(p => p.IsArchived), It.IsAny<CancellationToken>()), Times.Once);
    }
}