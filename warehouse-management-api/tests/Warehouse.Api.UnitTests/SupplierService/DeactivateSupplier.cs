using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Suppliers.Commands.DeactivateSupplier;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Api.UnitTests.SupplierService;

public class DeactivateSupplier
{
    private readonly Mock<ISupplierRepository> _supplierRepositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly DeactivateSupplierHandler _handler;

    public DeactivateSupplier()
    {
        _supplierRepositoryMock = new Mock<ISupplierRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        var loggerMock = new Mock<ILogger<DeactivateSupplierHandler>>();

        _handler = new DeactivateSupplierHandler(
            _supplierRepositoryMock.Object,
            loggerMock.Object,
            _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ValidSupplier_DeactivatesAndUpdates()
    {
        // Arrange
        var supplier = new SupplierBuilder().WithName("Acme Corp").Build();
        var command = new DeactivateSupplierCommand(supplier.SupplierId);

        _supplierRepositoryMock.Setup(repo => repo.GetByIdAsync(supplier.SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(supplier);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        supplier.IsActive.Should().BeFalse();
        
        _supplierRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<Supplier>(s => !s.IsActive), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(cache => cache.RemoveAsync("ListSuppliersHandler_ListSuppliersQuery", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingSupplier_ThrowsNotFoundException()
    {
        // Arrange
        var command = new DeactivateSupplierCommand("non-existent");

        _supplierRepositoryMock.Setup(repo => repo.GetByIdAsync("non-existent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Supplier?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        _supplierRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Supplier>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
