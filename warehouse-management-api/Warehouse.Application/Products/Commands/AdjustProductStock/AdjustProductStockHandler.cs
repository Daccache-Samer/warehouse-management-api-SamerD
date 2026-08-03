using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Warehouse.Application.Exceptions;
using Warehouse.Application.IntegrationEvents;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.AdjustProductStock;

public class AdjustProductStockHandler(
    IProductRepository productRepository, IMapper mapper,ILogger<AdjustProductStockHandler> logger,IDistributedCache cache,
    IEventPublisher eventPublisher, ICorrelationContext correlationContext, IConfiguration configuration)
    : IRequestHandler<AdjustProductStockCommand, ProductViewModel>
{
    public async Task<ProductViewModel> Handle(AdjustProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        var previousQuantity =  product.QuantityInStock;
        var delta = request.AdjustmentType == StockAdjustmentType.Increase ? request.Quantity : -request.Quantity;
        product.AdjustQuantity(delta);

        await productRepository.UpdateAsync(product, cancellationToken);
        await cache.RemoveAsync($"GetProductByIdQuery-{product.Id}", cancellationToken);
        await cache.RemoveAsync("ListProductsHandler_ListProductsQuery", cancellationToken);

        logger.LogInformation(
            "Stock adjusted: {ProductId} {Sku} {AdjustmentType} {Delta} -> new quantity {NewQuantity}. Reason: {Reason}",
            product.Id, product.SKU, request.AdjustmentType, delta, product.QuantityInStock, request.Reason ?? "n/a");
        
        await eventPublisher.PublishAsync(
            new StockAdjustedEvent
            {
                CorrelationId = correlationContext.CorrelationId,
                EventType = EventTypes.StockAdjusted,
                RelatedEntityId = product.Id,
                RelatedEntityType = "Product",
                Severity = "Info",
                Sku = product.SKU,
                ProductName = product.Name,
                AdjustmentType = request.AdjustmentType.ToString(),
                Delta = delta,
                NewQuantity = product.QuantityInStock,
                Reason = request.Reason
            },
           EventTypes.StockAdjusted,
            cancellationToken);

        var threshold = configuration.GetValue("WarehouseEvents:LowStockThreshold", 10);
        if (StockThresholdPolicy.CrossedIntoLowStock(previousQuantity, product.QuantityInStock, threshold))
        {
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

        return mapper.Map<ProductViewModel>(product);
    }
}