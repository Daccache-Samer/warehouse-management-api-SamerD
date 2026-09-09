using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using warehouse_management_api.Contracts;
using Warehouse.Api.IntegrationTests.TestUtilities.Helpers;
using Warehouse.Application.Products.ViewModels;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Api.IntegrationTests;

public class FullBusinessFlowTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public FullBusinessFlowTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("TestAuthScheme", "Admin");
    }

    [Fact]
    public async Task FullProductLifecycle_CreateSupplierThroughArchive_CompletesSuccessfully()
    {
        // Step 1 — create supplier
        var createSupplierResponse = await _client.PostAsJsonAsync("/api/suppliers", new CreateSupplierRequest
        {
            Name = "Acer",
            Country = "Lebanon",
            ContactEmail = "acer@example.com",
            PhoneNumber = "70123456"
        });
        createSupplierResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var supplier = await createSupplierResponse.Content.ReadFromJsonAsync<SupplierViewModel>();
        supplier.Should().NotBeNull();

        // Step 2 — create product
        var createProductResponse = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest
        {
            Name = "Gaming Laptop",
            Sku = "SKU-{111123213}",
            Description = "A gaming laptop",
            Price = 1500m,
            QuantityInStock = 20,
            ExpiryDate = DateTime.UtcNow.AddYears(1)
        });
        createProductResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await createProductResponse.Content.ReadFromJsonAsync<ProductViewModel>();
        product.Should().NotBeNull();

        // Step 3 — assign supplier
        var assignResponse = await _client.PostAsync(
            $"/api/products/{product.Id}/assign-supplier/{supplier.SupplierId}", content: null);
        assignResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterAssign = await assignResponse.Content.ReadFromJsonAsync<ProductViewModel>();
        afterAssign?.SupplierId.Should().Be(supplier.SupplierId);

        // Step 4 — upload image
        var imageContent = MultipartFormHelper.CreateFileContent(
            "file", "photo.jpg", "image/jpeg", "fake-jpg-bytes"u8.ToArray());
        var uploadResponse = await _client.PostAsync($"/api/products/{product.Id}/image", imageContent);
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using (var uploadScope = _factory.Services.CreateScope())
        {
            var uploadDb = uploadScope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
            var afterUpload = await uploadDb.Products
                .Include(p => p.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            afterUpload.Should().NotBeNull();
            afterUpload.Images.Should().ContainSingle(i => i.FileName == "photo.jpg");
        }

        // Step 5 — update quantity
        var quantityResponse = await _client.PostAsJsonAsync(
            $"/api/products/{product.Id}/quantity", new UpdateProductQuantityRequest { QuantityInStock = 5 });
        quantityResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterQuantity = await quantityResponse.Content.ReadFromJsonAsync<ProductViewModel>();
        afterQuantity?.QuantityInStock.Should().Be(5);

        // Step 6 — update price
        var priceResponse = await _client.PostAsJsonAsync(
            $"/api/products/{product.Id}/price", new UpdateProductPriceRequest { Price = 1299.99m });
        priceResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterPrice = await priceResponse.Content.ReadFromJsonAsync<ProductViewModel>();
        afterPrice?.Price.Should().Be(1299.99m);

        // Step 7 — archive product
        var archiveResponse = await _client.DeleteAsync($"/api/products/{product.Id}");
        archiveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 8 — verify archived state 
        var getAfterArchive = await _client.GetAsync($"/api/products/{product.Id}");
        getAfterArchive.StatusCode.Should().Be(HttpStatusCode.NotFound); 

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var persisted = await db.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        persisted.Should().NotBeNull();
        persisted.IsArchived.Should().BeTrue();
        persisted.SupplierId.Should().Be(supplier.SupplierId);
        persisted.QuantityInStock.Should().Be(5);
        persisted.Price.Should().Be(1299.99m);
        persisted.Images.Should().BeEmpty("ArchiveProductHandler cascades image deletion on archive");
    }
}