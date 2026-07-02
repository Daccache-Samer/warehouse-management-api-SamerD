namespace warehouse_management_api.Contracts;

public record CreateProductRequest(
    string Name,
    string SKU,
    string Description,
    int Price,
    int QuantityInStock,
    string SupplierName,
    DateTimeOffset ExpiryDate
);