namespace Warehouse.DomainWarehouse.Domain.Suppliers;

public class SupplierDocument
{
    public string SupplierDocumentId { get; private set; } = string.Empty;
    public string SupplierId { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ObjectKey { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }

    private SupplierDocument() { }

    public static SupplierDocument Create(string supplierId, string fileName, string objectKey) =>
        new()
        {
            SupplierDocumentId = Guid.NewGuid().ToString(),
            SupplierId = supplierId,
            FileName = fileName,
            ObjectKey = objectKey,
            UploadedAt = DateTime.UtcNow
        };
}