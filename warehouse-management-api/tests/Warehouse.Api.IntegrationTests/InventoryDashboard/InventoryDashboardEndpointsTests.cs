using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Warehouse.Api.IntegrationTests.TestUtilities.Helpers;
using Warehouse.Application.InventoryDashboard.ViewModels;

namespace Warehouse.Api.IntegrationTests.InventoryDashboard;

public class InventoryEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public InventoryEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
    }

    private void AuthenticateAsUser()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme, "User");
    }

    [Fact]
    public async Task GET_Dashboard_ReturnsCorrectLowStockMetrics()
    {
        // Arrange
        AuthenticateAsUser();
        await _factory.SeedAsync(db =>
        {
            try
            {
                var lowStockProduct = DomainWarehouse.Domain.Products.Product.Create("Low Stock Item", "SKU-LOW-001", "Desc", 10m, 5, DateTime.UtcNow.AddYears(1));
                
                var healthyStockProduct = DomainWarehouse.Domain.Products.Product.Create("Healthy Stock Item", "SKU-HLT-001", "Desc", 10m, 20, DateTime.UtcNow.AddYears(1));
                
                var archivedLowStock = DomainWarehouse.Domain.Products.Product.Create("Archived Item", "SKU-ARC-002", "Desc", 10m, 2, DateTime.UtcNow.AddYears(1));
                archivedLowStock.Archive();

                db.Products.AddRange(lowStockProduct, healthyStockProduct, archivedLowStock);
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });

        // Act
        var response = await _client.GetAsync("/api/inventory/dashboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var dashboard = await response.Content.ReadFromJsonAsync<InventoryDashboardViewModel>();
        dashboard.Should().NotBeNull();
        
        dashboard.LowStockProductCount.Should().Be(1); // Only 'lowStockProduct' should be counted
    }
}