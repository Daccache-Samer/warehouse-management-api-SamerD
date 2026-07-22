namespace Warehouse.Application.Suppliers.ViewModels;

public class SupplierDocumentViewModel
{
    public string SupplierDocumentId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
}