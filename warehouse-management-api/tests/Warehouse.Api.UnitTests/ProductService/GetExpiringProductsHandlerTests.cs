using AutoMapper;
using FluentAssertions;
using Moq;
using Warehouse.Application.Products.Queries.GetExpiringProducts;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Api.UnitTests.ProductService;

public class GetExpiringProductsHandlerTests
{
    private readonly Mock<IProductRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly GetExpiringProductsHandler _handler;

    public GetExpiringProductsHandlerTests()
    {
        _handler = new GetExpiringProductsHandler(_repositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldPassWithinDaysToRepository()
    {
        // Arrange
        var query = new GetExpiringProductsQuery(WithinDays: 15);
        _repositoryMock
            .Setup(r => r.GetExpiringProductsAsync(15, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Product>());
        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<ExpiringProductViewModel>>(It.IsAny<IReadOnlyList<Product>>()))
            .Returns(Array.Empty<ExpiringProductViewModel>());

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.GetExpiringProductsAsync(15, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoProductsAreExpiring()
    {
        // Arrange
        var query = new GetExpiringProductsQuery(WithinDays: 30);
        _repositoryMock
            .Setup(r => r.GetExpiringProductsAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Product>());
        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<ExpiringProductViewModel>>(It.IsAny<IReadOnlyList<Product>>()))
            .Returns(Array.Empty<ExpiringProductViewModel>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedViewModels_WhenProductsAreExpiring()
    {
        // Arrange
        var product = Product.Create("Milk", "SKU-001", "Dairy", 2.5m, 10, DateTime.UtcNow.AddDays(5));
        var expected = new ExpiringProductViewModel
        {
            Id = product.Id,
            Name = "Milk",
            SKU = "SKU-001",
            ExpiryDate = product.ExpiryDate,
            DaysUntilExpiry = 5
        };

        _repositoryMock
            .Setup(r => r.GetExpiringProductsAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });
        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<ExpiringProductViewModel>>(It.IsAny<IReadOnlyList<Product>>()))
            .Returns(new List<ExpiringProductViewModel> { expected });

        var query = new GetExpiringProductsQuery(WithinDays: 30);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(expected);
    }
}