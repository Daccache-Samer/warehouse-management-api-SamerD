namespace Warehouse.Application;

public record CreateProductRequest(
    string Name,
    string SKU,
    string Description,
    decimal Price,
    int QuantityInStock,
    string SupplierName,
    DateTime ExpiryDate
);