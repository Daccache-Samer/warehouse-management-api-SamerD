using MediatR;
using Microsoft.Extensions.Logging;
using Warehouse.Application.InventoryDashboard.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.InventoryDashboard.Queries;

public class GetInventoryDashboardHandler(
    IProductRepository productRepository, ISupplierRepository supplierRepository,ILogger<GetInventoryDashboardHandler> logger)
    : IRequestHandler<GetInventoryDashboardQuery, InventoryDashboardViewModel>
{
    private const int LowStockThreshold = 10; 

    public async Task<InventoryDashboardViewModel> 
        Handle(GetInventoryDashboardQuery request, CancellationToken cancellationToken)
    {
        var totalProducts = await TryGetMetric(() =>
            productRepository.CountAsync(cancellationToken), "TotalProducts");
        var lowStock = await TryGetMetric(() =>
            productRepository.CountLowStockAsync(LowStockThreshold, cancellationToken),"LowStock");
        var totalValue = await TryGetMetric(() =>
            productRepository.GetTotalInventoryValueAsync(cancellationToken),"TotalValue");
        var activeSuppliers = await TryGetMetric(() =>
            supplierRepository.CountActiveSuppliersAsync(cancellationToken),"ActiveSuppliers");

        return new InventoryDashboardViewModel(
            totalProducts,  lowStock, LowStockThreshold, totalValue, activeSuppliers, DateTime.UtcNow);
    }
            

        private async Task<T?> TryGetMetric<T>(Func<Task<T>> query, string metricName)
            where T : struct
        {
            try
            {
                return await query();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dashboard metric {Metric} failed to load", metricName);
                return null;
            }
        }
}
