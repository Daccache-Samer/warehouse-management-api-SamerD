namespace Warehouse.Application.Products.ViewModels;

public class ProductViewModel{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int QuantityInStock { get; init; }
    public string? SupplierId { get; init; }
}