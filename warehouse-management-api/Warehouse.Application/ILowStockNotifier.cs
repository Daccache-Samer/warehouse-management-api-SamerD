using Microsoft.Extensions.Configuration;
using Warehouse.Application.IntegrationEvents;
using Warehouse.Application.Products;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application;

public interface ILowStockNotifier
{
    Task NotifyIfCrossedThresholdAsync(Product product, int previousQuantity, CancellationToken cancellationToken);
}

public class LowStockNotifier(
    IEventPublisher eventPublisher, ICorrelationContext correlationContext, IConfiguration configuration)
    : ILowStockNotifier
{
    public async Task NotifyIfCrossedThresholdAsync(
        Product product, int previousQuantity, CancellationToken cancellationToken)
    {
        var threshold = configuration.GetValue("WarehouseEvents:LowStockThreshold", 10);
        if (!StockThresholdPolicy.CrossedIntoLowStock(previousQuantity, product.QuantityInStock, threshold))
        {
            return;
        }

        await eventPublisher.PublishAsync(
            new StockLowDetectedEvent
            {
                CorrelationId = correlationContext.CorrelationId,
                EventType = EventTypes.StockLow,
                RelatedEntityId = product.Id,
                RelatedEntityType = "Product",
                Severity = "Warning",
                Sku = product.SKU,
                ProductName = product.Name,
                CurrentQuantity = product.QuantityInStock,
                Threshold = threshold
            },
            EventTypes.StockLow,
            cancellationToken);
    }
}