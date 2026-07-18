using Microsoft.Extensions.Logging;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.BackgroundJobs;

public class ExpiryDateCheckJob(IProductRepository productRepository, ILogger<ExpiryDateCheckJob> logger)
{
    public async Task ExecuteAsync()
    {
        var products = await productRepository.GetExpiringProductsAsync(withinDays: 30);

        var now = DateTime.UtcNow;
        var expired = products.Where(p => p.ExpiryDate < now).ToList();
        var expiringSoon = products.Where(p => p.ExpiryDate >= now).ToList();

        logger.LogWarning(
            "Expiry check: {ExpiredCount} expired product(s), {SoonCount} expiring within 30 days.",
            expired.Count, expiringSoon.Count);

        if (expired.Count > 0)
            logger.LogWarning("Expired products: {Names}", string.Join(", ", expired.Select(p => p.Name)));

        if (expiringSoon.Count > 0)
            logger.LogInformation("Expiring soon: {Names}", string.Join(", ", expiringSoon.Select(p => p.Name)));
    }
}