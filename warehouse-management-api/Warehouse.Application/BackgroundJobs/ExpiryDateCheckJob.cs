using Microsoft.Extensions.Logging;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.BackgroundJobs;

public class ExpiryDateCheckJob(IProductRepository productRepository, ILogger<ExpiryDateCheckJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var products = await productRepository.GetExpiringProductsAsync(withinDays: 30, ct: cancellationToken);

        var now = DateTime.UtcNow;
        var expired = products.Where(p => p.ExpiryDate < now).ToList();
        var expiringSoon = products.Where(p => p.ExpiryDate >= now).ToList();

        logger.LogInformation(
            "Expiry check: {ExpiredCount} expired product(s), {SoonCount} expiring within 30 days.",
            expired.Count, expiringSoon.Count);

        if (expired.Count > 0)
            logger.LogWarning("Expired products: {Names}", string.Join(", ", expired.Select(p => p.Name)));

        if (expiringSoon.Count > 0)
            logger.LogInformation("Expiring soon: {Names}", string.Join(", ", expiringSoon.Select(p => p.Name)));
        
        var toArchive = expired.Where(p => p.ExpiryDate < now.AddDays(-7)).ToList();
        foreach (var product in toArchive)
        {
            product.Archive();
            await productRepository.UpdateAsync(product, cancellationToken);
            logger.LogWarning(
                "Product auto-archived (expired {DaysAgo} days ago): {ProductName} ({ProductId})",
                (int)(now - product.ExpiryDate).TotalDays,
                product.Name,
                product.Id);
        }
        if (toArchive.Count > 0)
            logger.LogWarning("Auto-archived {Count} product(s) due to expiry.", toArchive.Count);
    }
    
}