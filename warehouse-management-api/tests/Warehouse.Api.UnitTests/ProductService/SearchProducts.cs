using AutoMapper;
using FluentAssertions;
using Moq;
using Warehouse.Api.UnitTests.TestUtilities.Builders;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.Queries.SearchProducts;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Api.UnitTests.ProductService;

public class SearchProducts
{
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<ISupplierRepository> _supplierRepositoryMock = new();
    private readonly SearchProductsHandler _handler;

    public SearchProducts()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductViewModel>();
        });
        var mapper = mapperConfig.CreateMapper();

        _handler = new SearchProductsHandler(
            _productRepositoryMock.Object,
            _supplierRepositoryMock.Object,
            mapper);
    }

    [Fact]
    public async Task Handle_EmptyFilters_ThrowsValidationException()
    {
        // Arrange
        var query = new SearchProductsQuery(null, null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_SearchByName_ReturnsMatches()
    {
        // Arrange
        var query = new SearchProductsQuery("Laptop", null);
        var products = new List<Product>
        {
            new ProductBuilder().WithName("Gaming Laptop").Build(),
            new ProductBuilder().WithName("Office Mouse").Build()
        };

        _productRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Gaming Laptop");
    }

    [Fact]
    public async Task Handle_SearchBySupplier_ReturnsMatches()
    {
        // Arrange
        var query = new SearchProductsQuery(null, "Acme");
        
        var supplier = new SupplierBuilder().WithName("Acme Corp").Build();
        var supplierId = supplier.SupplierId;

        var products = new List<Product>
        {
            new ProductBuilder().WithName("Product 1").WithSupplierId(supplierId).Build(),
            new ProductBuilder().WithName("Product 2").WithSupplierId("other-id").Build()
        };

        _supplierRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Supplier> { supplier });

        _productRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Product 1");
    }

    [Fact]
    public async Task Handle_SearchByBothFilters_ReturnsIntersection()
    {
        // Arrange
        var query = new SearchProductsQuery("Laptop", "Acme");
        
        var supplier = new SupplierBuilder().WithName("Acme Corp").Build();
        var supplierId = supplier.SupplierId;

        var products = new List<Product>
        {
            new ProductBuilder().WithName("Gaming Laptop").WithSupplierId(supplierId).Build(), 
            new ProductBuilder().WithName("Office Laptop").WithSupplierId("other-id").Build(), 
            new ProductBuilder().WithName("Mouse").WithSupplierId(supplierId).Build() 
        };

        _supplierRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Supplier> { supplier });

        _productRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Gaming Laptop");
    }
}
