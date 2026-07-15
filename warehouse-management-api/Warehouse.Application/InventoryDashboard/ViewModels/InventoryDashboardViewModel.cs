namespace Warehouse.Application.InventoryDashboard.ViewModels;

public record InventoryDashboardViewModel(
    int TotalProductCount,
    int LowStockProductCount,
    int LowStockThreshold,
    decimal TotalInventoryValue,
    int ActiveSupplierCount,
    DateTime GeneratedAtUtc);