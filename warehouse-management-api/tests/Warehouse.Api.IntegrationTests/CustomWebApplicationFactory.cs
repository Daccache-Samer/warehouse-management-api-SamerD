using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Warehouse.Api.IntegrationTests.TestUtilities.Helpers;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.Infrastructure.Persistence;
using Microsoft.AspNetCore.TestHost;

namespace Warehouse.Api.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"WarehouseTestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContextOptions
            services.RemoveAll<DbContextOptions<WarehouseDbContext>>();
            services.RemoveAll<DbContextOptions>();

            services.AddDbContext<WarehouseDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
            
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultScheme = TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme, options => { });

            //intercept and override the Authorization pipeline
            services.AddTransient<IPolicyEvaluator, TestPolicyEvaluator>();

            // Swap Redis cache for in-memory
            services.RemoveAll<IDistributedCache>();
            services.AddDistributedMemoryCache();

            // Replace real infrastructure clients with in-process fakes
            services.RemoveAll<IEventPublisher>();
            services.AddSingleton<IEventPublisher, NoOpEventPublisher>();

            services.RemoveAll<IFileStorage>();
            services.AddSingleton<IFileStorage, InMemoryFileStorage>();
        });
    }

    public async Task SeedAsync(Func<WarehouseDbContext, Task> seed)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
        await seed(db);
        await db.SaveChangesAsync();
    }
}