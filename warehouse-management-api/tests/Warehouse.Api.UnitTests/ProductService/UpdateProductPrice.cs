using AutoMapper;
using FluentAssertions;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application.Products.Commands.UpdateProductPrice;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Exceptions;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Api.UnitTests.ProductService;

public class UpdateProductPrice
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly UpdateProductPriceHandler _handler;

    public UpdateProductPrice()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductViewModel>();
        });
        var mapper = mapperConfig.CreateMapper();

        _handler = new UpdateProductPriceHandler(
            _productRepositoryMock.Object,
            mapper);
    }

    [Fact]
    public async Task Handle_ValidPrice_UpdatesPriceAndReturnsViewModel()
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build();
        var command = new UpdateProductPriceCommand(product.Id, 250m);

        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(
                product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Price.Should().Be(250m);

        _productRepositoryMock.Verify(repo => repo.UpdateAsync(
            It.Is<Product>(p => p.Price == 250m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public async Task Handle_InvalidPrice_ThrowsDomainException(decimal invalidPrice)
    {
        // Arrange
        var product = new ProductBuilder().WithName("Laptop").Build();
        var command = new UpdateProductPriceCommand(product.Id, invalidPrice);

        _productRepositoryMock.Setup(repo =>
                repo.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
        _productRepositoryMock.Verify(repo =>
            repo.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}