using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Warehouse.Application.Suppliers.Commands.CreateSupplier;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Api.UnitTests.SupplierService;

public class CreateSupplier
{
    private readonly Mock<ISupplierRepository> _supplierRepositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly CreateSupplierHandler _handler;

    public CreateSupplier()
    {
        _supplierRepositoryMock = new Mock<ISupplierRepository>();
        var loggerMock = new Mock<ILogger<CreateSupplierHandler>>();
        _cacheMock = new Mock<IDistributedCache>();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Supplier, SupplierViewModel>();
        });
        var mapper = mapperConfig.CreateMapper();

        _handler = new CreateSupplierHandler(
            _supplierRepositoryMock.Object,
            mapper,
            loggerMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ValidSupplier_SucceedsAndReturnsViewModel()
    {
        // Arrange
        var command = new CreateSupplierCommand("Acme Corp", "USA", "acme@test.com", "+1-555-1234");
        
        Supplier? capturedSupplier = null;
        _supplierRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()))
            .Callback<Supplier, CancellationToken>((s, _) => capturedSupplier = s)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Acme Corp");

        capturedSupplier.Should().NotBeNull();
        capturedSupplier.SupplierId.Should().NotBeNullOrEmpty();
        capturedSupplier.Name.Should().Be("Acme Corp");

        _supplierRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(cache => cache.RemoveAsync("ListSuppliersHandler_ListSuppliersQuery", It.IsAny<CancellationToken>()), Times.Once);
    }
}
