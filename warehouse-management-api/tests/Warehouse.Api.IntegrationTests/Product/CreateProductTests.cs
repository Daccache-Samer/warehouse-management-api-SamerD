// Warehouse.Api.IntegrationTests/Products/CreateProductTests.cs

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using warehouse_management_api.Contracts;
using Warehouse.Application.Products;
using Warehouse.Application.Products.ViewModels;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Api.IntegrationTests.Product;

public class CreateProductTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CreateProductTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "TestAuthScheme admin");
    }

    private static CreateProductRequest ValidRequest(string? sku = null) => new()
    {
        Name = "Gaming Laptop",
        Sku = sku ?? $"SKU-{Guid.NewGuid():N}",
        Description = "A gaming laptop",
        Price = 1500m,
        QuantityInStock = 10,
        ExpiryDate = DateTime.UtcNow.AddYears(1)
    };

    [Fact]
    public async Task Create_WithValidPayload_Returns201WithLocationHeaderAndPersistsProduct()
    {
        var request = ValidRequest();

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.ToString().Should().Contain("/api/products/");

        var body = await response.Content.ReadFromJsonAsync<ProductViewModel>();
        body.Should().NotBeNull();
        body.Name.Should().Be(request.Name);
        body.SKU.Should().Be(request.Sku);
        body.Description.Should().Be(request.Description);
        body.Price.Should().Be(request.Price);
        body.QuantityInStock.Should().Be(request.QuantityInStock);
        body.SupplierId.Should().BeNull();
        body.Id.Should().NotBeNullOrWhiteSpace();

        response.Headers.Location.ToString().Should().EndWith(body.Id);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var persisted = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == body.Id);

        persisted.Should().NotBeNull();
        persisted.SKU.Should().Be(request.Sku);
        persisted.Name.Should().Be(request.Name);
        persisted.Price.Should().Be(request.Price);
        persisted.QuantityInStock.Should().Be(request.QuantityInStock);
        persisted.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task Create_WithDuplicateSku_Returns409AndDoesNotCreateSecondRow()
    {
        var sku = $"SKU-{Guid.NewGuid():N}";
        var first = await _client.PostAsJsonAsync("/api/products", ValidRequest(sku));
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await _client.PostAsJsonAsync("/api/products", ValidRequest(sku));

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var count = await db.Products.CountAsync(p => p.SKU == sku);
        count.Should().Be(1);
    }

    [Fact]
    public async Task Create_ListCacheEviction_IsBrokenDueToKeyMismatch()
    {
        // Documents a known bug rather than asserting desired behavior:
        // ProductCacheKeys.List(bool) ignores its parameter and always
        // returns "ListProductsHandler", but ListProductsHandler caches
        // under that literal key while CreateProductHandler evicts
        // "ListProductsHandler_ListProductsQuery" (a different string).
        // Net effect: the list cache is never actually invalidated on create.
        using var scope = _factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        var actualListCacheKey = ProductCacheKeys.List(false);

        await cache.SetStringAsync(actualListCacheKey, "[]");

        var response = await _client.PostAsJsonAsync("/api/products", ValidRequest());
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var stillCached = await cache.GetStringAsync(actualListCacheKey);
        stillCached.Should().Be("[]", because:
            "CreateProductHandler evicts a differently-named key and never clears the real one");
    }
}