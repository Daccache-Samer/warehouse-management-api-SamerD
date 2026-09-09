namespace Warehouse.Application.Products.ViewModels;

public class ExpiringProductViewModel : ProductViewModel
{
    public DateTime ExpiryDate { get; init; }
    public int DaysUntilExpiry { get; init; }
}