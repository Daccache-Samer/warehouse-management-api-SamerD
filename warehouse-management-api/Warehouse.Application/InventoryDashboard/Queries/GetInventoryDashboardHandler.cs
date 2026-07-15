using MediatR;
using Warehouse.Application.InventoryDashboard.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.InventoryDashboard.Queries;

public class GetInventoryDashboardHandler(IProductRepository productRepository, ISupplierRepository supplierRepository)
    : IRequestHandler<GetInventoryDashboardQuery, InventoryDashboardViewModel>
{
    private const int LowStockThreshold = 10; 

    public async Task<InventoryDashboardViewModel> Handle(GetInventoryDashboardQuery request, CancellationToken cancellationToken)
    {
        var totalProductsTask = productRepository.CountAsync(cancellationToken);
        var lowStockTask = productRepository.CountLowStockAsync(LowStockThreshold, cancellationToken);
        var totalValueTask = productRepository.GetTotalInventoryValueAsync(cancellationToken);
        var activeSuppliersTask = supplierRepository.CountActiveSuppliersAsync(cancellationToken);

        await Task.WhenAll(totalProductsTask, lowStockTask, totalValueTask, activeSuppliersTask);

        return new InventoryDashboardViewModel(
            TotalProductCount: await totalProductsTask,
            LowStockProductCount: await lowStockTask,
            LowStockThreshold: LowStockThreshold,
            TotalInventoryValue: await totalValueTask,
            ActiveSupplierCount: await activeSuppliersTask,
            GeneratedAtUtc: DateTime.UtcNow);
    }
}