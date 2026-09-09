// Warehouse.Api.IntegrationTests/Products/UploadProductImageTests.cs

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Api.IntegrationTests.TestUtilities.Helpers;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Api.IntegrationTests.Product;

public class UploadProductImageTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UploadProductImageTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("Authorization", "TestAuthScheme admin");
    }

    private async Task<string> SeedProductAsync(string? sku = null)
    {
        var product = DomainWarehouse.Domain.Products.Product.Create(
            "Gaming Laptop", sku ?? $"SKU-{Guid.NewGuid():N}", "desc", 1500m, 10, DateTime.UtcNow.AddYears(1));

        await _factory.SeedAsync(async db => { await db.Products.AddAsync(product); });
        return product.Id;
    }

    private static byte[] MinimalJpegBytes() =>
        [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0xFF, 0xD9];

    [Fact]
    public async Task UploadImage_ValidJpeg_Returns200_PersistsImageRow_AndStoresBlob()
    {
        var productId = await SeedProductAsync();
        using var content = MultipartFormHelper.CreateFileContent(
            "file", "cover.jpg", "image/jpeg", MinimalJpegBytes());

        var response = await _client.PostAsync($"/api/products/{productId}/image", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var body = await response.Content.ReadFromJsonAsync<ProductViewModel>();
        body.Should().NotBeNull();
        body.Id.Should().Be(productId);
        body.Name.Should().Be("Gaming Laptop");
        body.QuantityInStock.Should().Be(10);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var images = await db.ProductImages.AsNoTracking().Where(i => i.ProductId == productId).ToListAsync();

        images.Should().HaveCount(1);
        images[0].FileName.Should().Be("cover.jpg");
        images[0].ObjectKey.Should().Contain(productId);

        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        var (stream, contentType) = await storage.DownloadAsync(images[0].ObjectKey);
        contentType.Should().Be("image/jpeg");
        stream.Length.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("cover.gif", "image/gif")]
    [InlineData("cover.bmp", "image/bmp")]
    public async Task UploadImage_DisallowedExtensionOrContentType_Returns400AndPersistsNothing(
        string fileName, string contentType)
    {
        var productId = await SeedProductAsync();
        using var content = MultipartFormHelper.CreateFileContent("file", fileName, contentType, [1, 2, 3]);

        var response = await _client.PostAsync($"/api/products/{productId}/image", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        (await db.ProductImages.CountAsync(i => i.ProductId == productId)).Should().Be(0);
    }

    [Fact]
    public async Task UploadImage_ExceedsMaxSize_Returns400()
    {
        var productId = await SeedProductAsync();
        var oversized = new byte[2 * 1024 * 1024 + 1]; // 2MB + 1 byte
        using var content = MultipartFormHelper.CreateFileContent("file", "big.jpg", "image/jpeg", oversized);

        var response = await _client.PostAsync($"/api/products/{productId}/image", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        (await db.ProductImages.CountAsync(i => i.ProductId == productId)).Should().Be(0);
    }

    [Fact]
    public async Task UploadImage_ProductDoesNotExist_Returns404()
    {
        using var content = MultipartFormHelper.CreateFileContent(
            "file", "cover.jpg", "image/jpeg", MinimalJpegBytes());

        var response = await _client.PostAsync($"/api/products/{Guid.NewGuid()}/image", content);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UploadImage_NoFileProvided_Returns400FromControllerGuardClause()
    {
        using var emptyContent = new MultipartFormDataContent();
        var productId = await SeedProductAsync();

        var response = await _client.PostAsync($"/api/products/{productId}/image", emptyContent);

        // This short-circuits in the controller before MediatR is invoked
        // (file is null / file.Length == 0), so it never reaches AddProductImageHandler.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    
}