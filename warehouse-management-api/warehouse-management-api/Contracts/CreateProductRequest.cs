namespace warehouse_management_api.Contracts;

public record CreateProductRequest(
    string Name,
    string SKU,
    string Description,
    decimal Price,
    int QuantityInStock,
    string SupplierName,
    DateTime ExpiryDate
);