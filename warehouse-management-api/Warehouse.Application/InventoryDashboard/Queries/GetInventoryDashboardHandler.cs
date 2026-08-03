using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Warehouse.Application.InventoryDashboard.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.InventoryDashboard.Queries;

public class GetInventoryDashboardHandler(
    IProductRepository productRepository, ISupplierRepository supplierRepository,ILogger<GetInventoryDashboardHandler> logger,
    IConfiguration configuration)
    : IRequestHandler<GetInventoryDashboardQuery, InventoryDashboardViewModel>
{

    public async Task<InventoryDashboardViewModel> 
        Handle(GetInventoryDashboardQuery request, CancellationToken cancellationToken)
    {
        var threshold = configuration.GetValue("WarehouseEvents:LowStockThreshold", 10);
        var totalProducts = await TryGetMetric(() =>
            productRepository.CountAsync(cancellationToken), "TotalProducts");
        var lowStock = await TryGetMetric(() =>
            productRepository.CountLowStockAsync(threshold, cancellationToken),"LowStock");
        var totalValue = await TryGetMetric(() =>
            productRepository.GetTotalInventoryValueAsync(cancellationToken),"TotalValue");
        var activeSuppliers = await TryGetMetric(() =>
            supplierRepository.CountActiveSuppliersAsync(cancellationToken),"ActiveSuppliers");

        return new InventoryDashboardViewModel(
            totalProducts,  lowStock, threshold, totalValue, activeSuppliers, DateTime.UtcNow);
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
