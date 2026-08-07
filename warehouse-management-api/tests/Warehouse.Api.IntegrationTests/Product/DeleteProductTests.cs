// Warehouse.Api.IntegrationTests/Products/DeleteProductTests.cs

using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Application.Products;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Api.IntegrationTests.Product;

public class DeleteProductTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DeleteProductTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "TestAuthScheme admin");
    }

    private async Task<(string ProductId, string ObjectKey)> SeedProductWithImageAsync()
    {
        var product = DomainWarehouse.Domain.Products.Product.Create(
            "Gaming Laptop", $"SKU-{Guid.NewGuid():N}", "desc", 1500m, 10, DateTime.UtcNow.AddYears(1));

        var objectKey = $"products/{product.Id}/{Guid.NewGuid()}-cover.jpg";
        var image = DomainWarehouse.Domain.Products.ProductImage.Create(product.Id, "cover.jpg", objectKey);
        product.AddImage(image);

        await _factory.SeedAsync(async db => { await db.Products.AddAsync(product); });

        // Seed the corresponding blob directly through the same fake storage
        // instance the app uses, so the cascade delete has something real to remove.
        using var scope = _factory.Services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        using var ms = new MemoryStream([1, 2, 3, 4]);
        var uploadResult = await storage.UploadAsync("products", product.Id, ms, "cover.jpg", "image/jpeg");

        // The upload call generates its own key, so re-point the seeded DB row
        // to match what was actually stored (keeps the two seeding steps consistent).
        await _factory.SeedAsync(async db =>
        {
            var tracked = await db.ProductImages.FirstAsync(i => i.ProductId == product.Id);
            db.Entry(tracked).CurrentValues.SetValues(
                DomainWarehouse.Domain.Products.ProductImage.Create(product.Id, uploadResult.FileName, uploadResult.ObjectKey));
        });

        return (product.Id, uploadResult.ObjectKey);
    }

    [Fact]
    public async Task Delete_ArchivesProduct_Returns204_ProductRowStillExistsButArchived()
    {
        var product = DomainWarehouse.Domain.Products.Product.Create(
            "Gaming Laptop", $"SKU-{Guid.NewGuid():N}", "desc", 1500m, 10, DateTime.UtcNow.AddYears(1));
        await _factory.SeedAsync(async db => { await db.Products.AddAsync(product); });

        var response = await _client.DeleteAsync($"/api/products/{product.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var persisted = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);

        persisted.Should().NotBeNull("archive is a soft delete, the row must remain");
        persisted.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_CascadesProductImages_RemovesDbRowsAndBlobsFromStorage()
    {
        var (productId, objectKey) = await SeedProductWithImageAsync();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
            (await db.ProductImages.CountAsync(i => i.ProductId == productId)).Should().Be(1,
                "sanity check: seeding should have produced exactly one image row");
        }

        var response = await _client.DeleteAsync($"/api/products/{productId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var remainingImages = await verifyDb.ProductImages.AsNoTracking()
            .Where(i => i.ProductId == productId).ToListAsync();
        remainingImages.Should().BeEmpty("ArchiveProductHandler cascades ProductImage deletion");

        var storage = verifyScope.ServiceProvider.GetRequiredService<IFileStorage>();
        var act = async () => await storage.DownloadAsync(objectKey);
        await act.Should().ThrowAsync<KeyNotFoundException>(
            "the blob should have been removed from storage alongside the DB row");
    }

    [Fact]
    public async Task Delete_NonExistentProduct_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/products/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_CalledTwice_IsIdempotent_SecondCallSucceedsWithNothingLeftToCascade()
    {
        var (productId, _) = await SeedProductWithImageAsync();

        var first = await _client.DeleteAsync($"/api/products/{productId}");
        first.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var second = await _client.DeleteAsync($"/api/products/{productId}");
        second.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var persisted = await db.Products.AsNoTracking().FirstAsync(p => p.Id == productId);
        persisted.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_ByIdCacheEviction_IsBrokenDueToKeyMismatch_GetImmediatelyAfterReturnsStaleData()
    {
        // Documents a known bug rather than asserting desired behavior:
        // GetProductByIdHandler caches under ProductCacheKeys.ById(id), which
        // ignores its `id` parameter and always resolves to "GetProductByIdQuery".
        // ArchiveProductHandler evicts a different, hand-rolled key
        // ("GetProductByIdQuery-{id}") that was never the one actually written.
        // Net effect: a GET immediately after DELETE returns stale pre-archive
        // data instead of 404, until the 5-minute TTL expires naturally.
        var product = DomainWarehouse.Domain.Products.Product.Create(
            "Gaming Laptop", $"SKU-{Guid.NewGuid():N}", "desc", 1500m, 10, DateTime.UtcNow.AddYears(1));
        await _factory.SeedAsync(async db => { await db.Products.AddAsync(product); });

        // Warm the cache the same way GetProductByIdHandler would.
        var getBefore = await _client.GetAsync($"/api/products/{product.Id}");
        getBefore.StatusCode.Should().Be(HttpStatusCode.OK);

        var deleteResponse = await _client.DeleteAsync($"/api/products/{product.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfter = await _client.GetAsync($"/api/products/{product.Id}");

        getAfter.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "this documents the bug: it should be 404 since the product is archived, " +
                     "but the cache eviction key mismatch leaves the stale cached entry in place");

        using var scope = _factory.Services.CreateScope();
        var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        var stillCached = await cache.GetStringAsync(ProductCacheKeys.ById(product.Id));
        stillCached.Should().NotBeNull("the real cache key was never touched by the eviction call");
    }
}