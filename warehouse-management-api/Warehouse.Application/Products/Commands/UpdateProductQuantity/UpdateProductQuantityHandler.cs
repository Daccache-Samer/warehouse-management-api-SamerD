using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Warehouse.Application.Exceptions;
using Warehouse.Application.IntegrationEvents;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.UpdateProductQuantity;

public class UpdateProductQuantityHandler(IProductRepository productRepository,IMapper mapper,IDistributedCache cache
    ,IEventPublisher eventPublisher,ICorrelationContext correlationContext,IConfiguration configuration)
    : IRequestHandler<UpdateProductQuantityCommand, ProductViewModel>
{
    public async Task<ProductViewModel> Handle(UpdateProductQuantityCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        var previousQuantity = product.QuantityInStock;
        product.UpdateQuantity(request.QuantityInStock);

        await productRepository.UpdateAsync(product, cancellationToken);
        await cache.RemoveAsync($"GetProductByIdQuery-{product.Id}", cancellationToken);
        await cache.RemoveAsync("ListProductsHandler_ListProductsQuery", cancellationToken);

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