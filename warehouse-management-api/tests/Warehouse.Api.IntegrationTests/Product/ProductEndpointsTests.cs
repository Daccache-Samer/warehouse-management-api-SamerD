using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using warehouse_management_api.Contracts;
using Warehouse.Api.IntegrationTests.TestUtilities.Helpers;
using Warehouse.Application.Products.ViewModels;

namespace Warehouse.Api.IntegrationTests.Product;

public class ProductEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ProductEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
    }

    private void AuthenticateAsAdmin()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            TestAuthHandler.AuthenticationScheme, "Admin");
    }

    private void AuthenticateAsUser()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            TestAuthHandler.AuthenticationScheme, "User");
    }

    [Fact]
    public async Task GET_GetById_InvalidId_Returns404NotFound()
    {
        // Arrange
        AuthenticateAsUser();
        var invalidId = Guid.NewGuid().ToString();

        // Act
        var response = await _client.GetAsync($"/api/products/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task GET_GetAll_OnlyAvailable_ReturnsFilteredProducts()
    {
        // Arrange
        AuthenticateAsUser();
        await _factory.SeedAsync(db =>
        {
            try
            {
                var p1 = DomainWarehouse.Domain.Products.Product.Create(
                    "Available Product", "SKU-AVL-001", "Desc", 10m, 100, DateTime.UtcNow.AddYears(1));
                var p2 = DomainWarehouse.Domain.Products.Product.Create(
                    "Available Product2", "SKU-ZER-001", "Desc", 10m, 10, DateTime.UtcNow.AddYears(1));
            
                var p3 = DomainWarehouse.Domain.Products.Product.Create(
                    "Archived Product", "SKU-ARC-001", "Desc", 10m, 100, DateTime.UtcNow.AddYears(1));
                p3.Archive();

                db.Products.AddRange(p1, p2, p3);
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });

        // Act 
        var response = await _client.GetAsync("/api/products?onlyAvailable=true");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductViewModel>>();
        
        products.Should().NotBeNull();
        products.Should().Contain(p => p.Name == "Available Product");
        products.Should().Contain(p => p.Name == "Available Product2");
        products.Should().NotContain(p => p.Name == "Archived Product");
    }

    [Fact]
    public async Task GET_Search_ByName_ReturnsMatchingProducts()
    {
        // Arrange
        AuthenticateAsUser();
        await _factory.SeedAsync(db =>
        {
            try
            {
                var p1 = DomainWarehouse.Domain.Products.Product.Create(
                    "Gaming Mouse", "SKU-MSE-001", "Desc", 50m, 10, DateTime.UtcNow.AddYears(1));
                var p2 = DomainWarehouse.Domain.Products.Product.Create(
                    "Mechanical Keyboard", "SKU-KBD-001", "Desc", 100m, 10, DateTime.UtcNow.AddYears(1));
                db.Products.AddRange(p1, p2);
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });

        // Act
        var response = await _client.GetAsync("/api/products/search?name=Gaming");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductViewModel>>();
        
        products.Should().NotBeNull();
        products.Should().ContainSingle();
        products[0].Name.Should().Be("Gaming Mouse");
    }

    [Fact]
    public async Task GET_ServerTime_WithCulture_ReturnsFormattedTime()
    {
        // Arrange
        AuthenticateAsUser();
        _client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr-FR"));

        // Act
        var response = await _client.GetAsync("/api/products/server-time");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("fr-FR");
    }

    [Fact]
    public async Task POST_Create_ValidProduct_Returns201Created()
    {
        // Arrange
        AuthenticateAsAdmin();
        var request = new CreateProductRequest
        {
            Name = "Integration Test Product",
            Sku = "SKU-INT-001",
            Description = "A product created during integration testing",
            Price = 299.99m,
            QuantityInStock = 50,
            SupplierName = "Test Supplier",
            ExpiryDate = DateTime.UtcNow.AddYears(2)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdProduct = await response.Content.ReadFromJsonAsync<ProductViewModel>();
        createdProduct.Should().NotBeNull();
        createdProduct.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task POST_Create_DuplicateSku_Returns409Conflict()
    {
        // Arrange
        AuthenticateAsAdmin();
        const string sku = "SKU-DUP-001";
        
        await _factory.SeedAsync(db =>
        {
            try
            {
                var product = DomainWarehouse.Domain.Products.Product.Create(
                    "First Product", sku, "Desc", 100m, 10, DateTime.UtcNow.AddYears(1));
                db.Products.Add(product);
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });

        var request = new CreateProductRequest
        {
            Name = "Second Product",
            Sku = sku,
            Description = "Desc",
            Price = 150m,
            QuantityInStock = 20,
            SupplierName = "Test Supplier",
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task POST_UpdateQuantity_ValidProduct_Returns200Ok()
    {
        // Arrange
        AuthenticateAsAdmin();
        var product = DomainWarehouse.Domain.Products.Product.Create(
            "Qty Product", "SKU-QTY-001", "Desc", 100m, 10, DateTime.UtcNow.AddYears(1));
        await _factory.SeedAsync(db =>
        {
            try
            {
                return Task.FromResult(db.Products.Add(product));
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });

        var request = new UpdateProductQuantityRequest { QuantityInStock = 25 };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/products/{product.Id}/quantity", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_UpdatePrice_ValidProduct_Returns200Ok()
    {
        // Arrange
        AuthenticateAsAdmin();
        var product = DomainWarehouse.Domain.Products.Product.Create(
            "Price Product", "SKU-PRICE-001", "Desc", 100m, 10, DateTime.UtcNow.AddYears(1));
        await _factory.SeedAsync(db =>
        {
            try
            {
                return Task.FromResult(db.Products.Add(product));
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });

        var request = new UpdateProductPriceRequest { Price = 199.99m };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/products/{product.Id}/price", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DELETE_DeleteProduct_Returns204NoContent_AndArchives()
    {
        // Arrange
        AuthenticateAsAdmin();
        var product = DomainWarehouse.Domain.Products.Product.Create(
            "Delete Product", "SKU-DEL-001", "Desc", 100m, 10, DateTime.UtcNow.AddYears(1));
        await _factory.SeedAsync(db =>
        {
            try
            {
                return Task.FromResult(db.Products.Add(product));
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });

        // Act
        var response = await _client.DeleteAsync($"/api/products/{product.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it still exists but is archived 
        AuthenticateAsUser();
        var getResponse = await _client.GetAsync($"/api/products/{product.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}