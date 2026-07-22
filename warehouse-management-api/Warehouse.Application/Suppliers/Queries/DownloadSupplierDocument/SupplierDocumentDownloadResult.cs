namespace Warehouse.Application.Suppliers.Queries.DownloadSupplierDocument;

public record SupplierDocumentDownloadResult(Stream Content, string ContentType, string FileName);