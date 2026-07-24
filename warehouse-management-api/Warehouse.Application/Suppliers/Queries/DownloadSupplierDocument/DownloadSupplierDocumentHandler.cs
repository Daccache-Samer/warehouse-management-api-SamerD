using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Queries.DownloadSupplierDocument;

public class DownloadSupplierDocumentHandler(ISupplierRepository supplierRepository, IFileStorage fileStorage)
    : IRequestHandler<DownloadSupplierDocumentQuery, SupplierDocumentDownloadResult>
{
    public async Task<SupplierDocumentDownloadResult> Handle(DownloadSupplierDocumentQuery request, CancellationToken ct)
    {
        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, ct)
                       ?? throw new NotFoundException($"Supplier with id '{request.SupplierId}' was not found.");

        var document = supplier.Documents.FirstOrDefault(d => d.SupplierDocumentId == request.DocumentId)
                       ?? throw new NotFoundException($"Document with id '{request.DocumentId}' was not found.");

        var (content, contentType) = await fileStorage.DownloadAsync(document.ObjectKey, ct);
        return new SupplierDocumentDownloadResult(content, contentType, document.FileName);
    }
}