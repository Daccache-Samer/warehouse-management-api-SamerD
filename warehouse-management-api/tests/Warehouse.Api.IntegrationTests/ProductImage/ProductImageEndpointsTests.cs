using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Api.IntegrationTests.TestUtilities.Helpers;
using Warehouse.Infrastructure.Persistence;

namespace Warehouse.Api.IntegrationTests.ProductImage;

public class ProductImageEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ProductImageEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("TestAuthScheme", "Admin");
    }

    private async Task<DomainWarehouse.Domain.Products.Product> SeedProductAsync()
    {
        var product = DomainWarehouse.Domain.Products.Product.Create(
            "Laptop", $"SKU-{Guid.NewGuid():N}", "A laptop", 999.99m, 10, DateTime.UtcNow.AddYears(1));
        await _factory.SeedAsync(async db => { await db.Products.AddAsync(product); });
        return product;
    }

    [Fact]
    public async Task POST_UploadImage_ValidJpg_Returns200AndPersistsImage()
    {
        // Arrange
        var product = await SeedProductAsync();
        var content = MultipartFormHelper.CreateFileContent(
            "file", "photo.jpg", "image/jpeg", [.. "fake-jpg-bytes"u8]);

        // Act
        var response = await _client.PostAsync($"/api/products/{product.Id}/image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var images = await db.ProductImages.Where(pi => pi.ProductId == product.Id).ToListAsync();
        images.Should().ContainSingle(pi => pi.FileName == "photo.jpg");
    }

    [Fact]
    public async Task POST_UploadImage_ValidPng_Returns200AndPersistsImage()
    {
        // Arrange
        var product = await SeedProductAsync();
        var content = MultipartFormHelper.CreateFileContent(
            "file", "photo.png", "image/png", [.. "fake-png-bytes"u8]);

        // Act
        var response = await _client.PostAsync($"/api/products/{product.Id}/image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        var images = await db.ProductImages.Where(pi => pi.ProductId == product.Id).ToListAsync();
        images.Should().ContainSingle(pi => pi.FileName == "photo.png");
    }

    [Fact]
    public async Task POST_UploadImage_TxtFile_Returns400BadRequest()
    {
        // Arrange
        var product = await SeedProductAsync();
        var content = MultipartFormHelper.CreateFileContent(
            "file", "notes.txt", "text/plain", [.. "just some text"u8]);

        // Act
        var response = await _client.PostAsync($"/api/products/{product.Id}/image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task POST_UploadImage_OversizedFile_Returns400BadRequest()
    {
        // Arrange
        var product = await SeedProductAsync();
        var oversizedBytes = new byte[3 * 1024 * 1024]; // 3 MB > 2 MB handler limit
        var content = MultipartFormHelper.CreateFileContent(
            "file", "big.jpg", "image/jpeg", oversizedBytes);

        // Act
        var response = await _client.PostAsync($"/api/products/{product.Id}/image", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}