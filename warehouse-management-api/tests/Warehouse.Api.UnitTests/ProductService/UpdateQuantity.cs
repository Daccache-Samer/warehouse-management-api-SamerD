using AutoMapper;
using FluentAssertions;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application;
using Warehouse.Application.Products.Commands.UpdateProductQuantity;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Api.UnitTests.ProductService;

public class UpdateQuantity
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<ILowStockNotifier> _lowStockNotifierMock = new();
    private readonly UpdateProductQuantityHandler _handler;

    public UpdateQuantity()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductViewModel>();
        });
        var mapper = mapperConfig.CreateMapper();

        _handler = new UpdateProductQuantityHandler(
            _productRepositoryMock.Object,
            mapper,
            _lowStockNotifierMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuantity_UpdatesStockAndReturnsViewModel()
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build();
        var command = new UpdateProductQuantityCommand(product.Id, 50);

        _productRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.QuantityInStock.Should().Be(50);

        _productRepositoryMock.Verify(repo => repo.UpdateAsync(
            It.Is<Product>(p => p.QuantityInStock == 50), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidQuantity_DelegatesLowStockCheckToNotifierWithPreviousAndUpdatedProduct()
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build(); // starts at QuantityInStock = 10
        const int previousQuantity = 10;
        var command = new UpdateProductQuantityCommand(product.Id, 50);

        _productRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — handler's only responsibility here is to hand off the *previous*
        // quantity (captured before mutation) alongside the now-updated product.
        // The threshold-crossing decision itself belongs to LowStockNotifier.
        _lowStockNotifierMock.Verify(n => n.NotifyIfCrossedThresholdAsync(
            It.Is<Product>(p => p.QuantityInStock == 50),
            previousQuantity,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NegativeQuantity_ThrowsDomainException()
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build();
        var command = new UpdateProductQuantityCommand(product.Id, -5);

        _productRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
        _productRepositoryMock.Verify(repo =>
            repo.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _lowStockNotifierMock.Verify(n => n.NotifyIfCrossedThresholdAsync(
            It.IsAny<Product>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidQuantity_UpdatesLastUpdatedAt()
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build();
        var initialDate = product.LastUpdatedAt;

        await Task.Delay(10);

        var command = new UpdateProductQuantityCommand(product.Id, 20);

        _productRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        Product? capturedProduct = null;
        _productRepositoryMock.Setup(repo =>
                repo.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => capturedProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct.LastUpdatedAt.Should().BeAfter(initialDate);
    }
}