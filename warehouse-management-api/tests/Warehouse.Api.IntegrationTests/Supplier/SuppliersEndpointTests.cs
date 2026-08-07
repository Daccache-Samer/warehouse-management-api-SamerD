using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using warehouse_management_api.Contracts;
using Warehouse.Application.Products.ViewModels;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Api.IntegrationTests.Supplier;

public class SuppliersEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SuppliersEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("TestAuthScheme", "Admin");
    }

    [Fact]
    public async Task POST_CreateSupplier_ValidPayload_Returns201Created()
    {
        // Arrange
        var request = new CreateSupplierRequest
        {
            Name = "Acer",
            Country = "Lebanon",
            ContactEmail = "acer@example.com",
            PhoneNumber = "+96170123456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/suppliers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<SupplierViewModel>();
        created.Should().NotBeNull();
        created.Name.Should().Be("Acer");
        created.SupplierId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GET_GetSupplierById_ExistingSupplier_ReturnsSupplier()
    {
        // Arrange
        var supplier = DomainWarehouse.Domain.Suppliers.Supplier.Create(
            "Dell", "USA", "dell@example.com", "+19725551234");
        await _factory.SeedAsync(async db => { await db.Suppliers.AddAsync(supplier); });

        // Act
        var response = await _client.GetAsync($"/api/suppliers/{supplier.SupplierId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<SupplierViewModel>();
        result.Should().NotBeNull();
        result.SupplierId.Should().Be(supplier.SupplierId);
        result.Name.Should().Be("Dell");
    }

    [Fact]
    public async Task DELETE_DeactivateSupplier_ExistingSupplier_Returns204AndMarksInactive()
    {
        // Arrange
        var supplier = DomainWarehouse.Domain.Suppliers.Supplier.Create(
            "HP", "USA", "hp@example.com", "+19725559999");
        await _factory.SeedAsync(async db => { await db.Suppliers.AddAsync(supplier); });

        // Act
        var response = await _client.DeleteAsync($"/api/suppliers/{supplier.SupplierId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // SupplierViewModel has no IsActive field so we are verifying the change straight from dbcontext
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var persisted = await db.Suppliers.FindAsync(supplier.SupplierId);
        persisted.Should().NotBeNull();
        persisted.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task POST_AssignSupplierToProduct_ActiveSupplier_ReturnsUpdatedProduct()
    {
        // Arrange
        var supplier = DomainWarehouse.Domain.Suppliers.Supplier.Create(
            "Lenovo", "China", "lenovo@example.com", "+8613800000000");
        var product = DomainWarehouse.Domain.Products.Product.Create(
            "Laptop", $"SKU-{Guid.NewGuid():N}", "A laptop", 999.99m, 10, DateTime.UtcNow.AddYears(1));

        await _factory.SeedAsync(async db =>
        {
            await db.Suppliers.AddAsync(supplier);
            await db.Products.AddAsync(product);
        });

        // Act
        var response = await _client.PostAsync(
            $"/api/products/{product.Id}/assign-supplier/{supplier.SupplierId}", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProductViewModel>();
        result.Should().NotBeNull();
        result.SupplierId.Should().Be(supplier.SupplierId);
    }
}