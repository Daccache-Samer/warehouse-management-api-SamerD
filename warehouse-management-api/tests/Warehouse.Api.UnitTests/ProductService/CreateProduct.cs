using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application.Exceptions;
using Warehouse.Application.IntegrationEvents;
using Warehouse.Application.Products.Commands.CreateProduct;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Api.UnitTests.ProductService;

public class CreateProduct
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly CreateProductHandler _handler;

    public CreateProduct()
    {
        var loggerMock = new Mock<ILogger<CreateProductHandler>>();
        var correlationContextMock = new Mock<ICorrelationContext>();
        
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductViewModel>();
        });
        var mapper = mapperConfig.CreateMapper();

        _handler = new CreateProductHandler(
            _productRepositoryMock.Object,
            mapper,
            loggerMock.Object,
            _cacheMock.Object,
            _eventPublisherMock.Object,
            correlationContextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidProduct_SucceedsAndReturnsViewModel()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product", "SKU-123", "Test Desc", 100m, 10, DateTime.UtcNow.AddYears(1));
        _productRepositoryMock.Setup(repo => 
                repo.GetBySkuAsync(command.SKU, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Product");
        result.SKU.Should().Be("SKU-123");

        _productRepositoryMock.Verify(repo => 
            repo.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventPublisherMock.Verify(pub => pub.PublishAsync(
            It.IsAny<ProductCreatedEvent>(), EventTypes.ProductCreated, 
            It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(cache => cache.RemoveAsync(
            "ListProductsHandler_ListProductsQuery", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSku_ThrowsConflictException()
    {
        // Arrange
        var command = new CreateProductCommand(
            "Test Product", "SKU-DUP", "Test Desc", 100m, 10, DateTime.UtcNow.AddYears(1));
        var existingProduct = new ProductBuilder().WithSku("SKU-DUP").Build();
        
        _productRepositoryMock.Setup(repo => 
                repo.GetBySkuAsync(command.SKU, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => 
            _handler.Handle(command, CancellationToken.None));
        _productRepositoryMock.Verify(repo => 
            repo.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidProduct_AssignsCreatedDateAndGeneratedId()
    {
        // Arrange
        var command = new CreateProductCommand("Test Product", "SKU-123", "Test Desc", 100m, 10, DateTime.UtcNow.AddYears(1));
        _productRepositoryMock.Setup(repo => repo.GetBySkuAsync(
                command.SKU, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        Product? capturedProduct = null;
        _productRepositoryMock.Setup(repo => repo.AddAsync(
                It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, ct) => capturedProduct = p)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedProduct.Should().NotBeNull();
        capturedProduct.Id.Should().NotBeNullOrEmpty();
        capturedProduct.CreatedAt.Should().NotBe(default(DateTime));
        // Verify it was created recently
        capturedProduct.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
