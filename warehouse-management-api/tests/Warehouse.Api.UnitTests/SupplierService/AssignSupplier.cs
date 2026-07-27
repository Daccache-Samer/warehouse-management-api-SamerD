using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.Commands.AssignSupplierToProduct;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Api.UnitTests.SupplierService;

public class AssignSupplier
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<ISupplierRepository> _supplierRepositoryMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly AssignSupplierToProductHandler _handler;

    public AssignSupplier()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductViewModel>();
        });
        var mapper = mapperConfig.CreateMapper();

        _handler = new AssignSupplierToProductHandler(
            _productRepositoryMock.Object,
            _supplierRepositoryMock.Object,
            mapper,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ValidSupplierAndProduct_AssignsAndReturnsViewModel()
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build();
        var supplier = new SupplierBuilder().WithName("Acme Corp").Build();
        var command = new AssignSupplierToProductCommand(product.Id, supplier.SupplierId);

        _productRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _supplierRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(supplier.SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        product.SupplierId.Should().Be(supplier.SupplierId);
        
        _productRepositoryMock.Verify(repo => repo.UpdateAsync(
            It.Is<Product>(p => p.SupplierId == supplier.SupplierId), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(cache => cache.RemoveAsync(
            $"GetProductByIdQuery-{product.Id}", It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(cache => cache.RemoveAsync(
            "ListProductsHandler_ListProductsQuery", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ArchivedProduct_ThrowsDomainException()
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build();
        product.Archive();
        var supplier = new SupplierBuilder().WithName("Acme Corp").Build();
        var command = new AssignSupplierToProductCommand(product.Id, supplier.SupplierId);

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(
                product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _supplierRepositoryMock.Setup(repo => repo.GetByIdAsync(
                supplier.SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
        _productRepositoryMock.Verify(repo => repo.UpdateAsync(
            It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MissingSupplier_ThrowsNotFoundException()
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build();
        var command = new AssignSupplierToProductCommand(product.Id, "missing-supplier");

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(
                product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _supplierRepositoryMock.Setup(repo => repo.GetByIdAsync(
                "missing-supplier", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _productRepositoryMock.Verify(repo => repo.UpdateAsync(
            It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MissingProduct_ThrowsNotFoundException()
    {
        // Arrange
        var supplier = new SupplierBuilder().WithName("Acme Corp").Build();
        var command = new AssignSupplierToProductCommand("missing-product", supplier.SupplierId);

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(
                "missing-product", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }
}
