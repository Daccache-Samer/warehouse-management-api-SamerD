using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Commands.DeleteSupplierDocument;

public class DeleteSupplierDocumentHandler(
    ISupplierRepository supplierRepository, IFileStorage fileStorage, IDistributedCache cache)
    : IRequestHandler<DeleteSupplierDocumentCommand>
{
    public async Task Handle(DeleteSupplierDocumentCommand request, CancellationToken ct)
    {
        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, ct)
                       ?? throw new NotFoundException($"Supplier with id '{request.SupplierId}' was not found.");

        var document = supplier.Documents.FirstOrDefault(d => d.SupplierDocumentId == request.DocumentId)
                       ?? throw new NotFoundException($"Document with id '{request.DocumentId}' was not found.");

        await fileStorage.DeleteAsync(document.ObjectKey, ct);
        supplier.RemoveDocument(document);
        await supplierRepository.UpdateAsync(supplier, ct);

        await cache.RemoveAsync(SupplierCacheKeys.ById(supplier.SupplierId), ct);
        await cache.RemoveAsync(SupplierCacheKeys.List, ct);
    }
}

