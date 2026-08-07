using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application.Products.Commands.UpdateProductQuantity;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Api.UnitTests.ProductService;

public class UpdateQuantity
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly UpdateProductQuantityHandler _handler;

    public UpdateQuantity()
    {
        var eventPublisherMock = new Mock<IEventPublisher>();
        var correlationContextMock = new Mock<ICorrelationContext>();
        var configurationMock = new Mock<IConfiguration>();
        var configSectionMock = new Mock<IConfigurationSection>();
        configSectionMock.Setup(s => s.Value).Returns("10");
        configurationMock.Setup(c => c.GetSection(
            "WarehouseEvents:LowStockThreshold")).Returns(configSectionMock.Object);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductViewModel>();
        });
        var mapper = mapperConfig.CreateMapper();

        _handler = new UpdateProductQuantityHandler(
            _productRepositoryMock.Object,
            mapper,
            _cacheMock.Object,
            eventPublisherMock.Object,
            correlationContextMock.Object,
            configurationMock.Object);
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
        _cacheMock.Verify(cache => cache.RemoveAsync(
            $"GetProductByIdQuery-{product.Id}", It.IsAny<CancellationToken>()), Times.Once);
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
