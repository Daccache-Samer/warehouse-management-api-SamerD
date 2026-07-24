using MediatR;

namespace Warehouse.Application.Suppliers.Queries.DownloadSupplierDocument;

public record DownloadSupplierDocumentQuery(string SupplierId, string DocumentId) : IRequest<SupplierDocumentDownloadResult>;
