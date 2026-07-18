using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.AdjustProductStock;

public class AdjustProductStockHandler(
    IProductRepository productRepository, IMapper mapper,ILogger<AdjustProductStockHandler> logger)
    : IRequestHandler<AdjustProductStockCommand, ProductViewModel>
{
    public async Task<ProductViewModel> Handle(AdjustProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        var delta = request.AdjustmentType == StockAdjustmentType.Increase ? request.Quantity : -request.Quantity;
        product.AdjustQuantity(delta);

        await productRepository.UpdateAsync(product, cancellationToken);
        logger.LogInformation(
            "Stock adjusted: {ProductId} {Sku} {AdjustmentType} {Delta} -> new quantity {NewQuantity}. Reason: {Reason}",
            product.Id, product.SKU, request.AdjustmentType, delta, product.QuantityInStock, request.Reason ?? "n/a");

        return mapper.Map<ProductViewModel>(product);
    }
}