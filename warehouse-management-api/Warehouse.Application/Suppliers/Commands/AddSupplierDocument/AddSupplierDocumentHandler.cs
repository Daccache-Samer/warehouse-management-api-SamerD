using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.DomainWarehouse.Domain.Common;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Commands.AddSupplierDocument;

public class AddSupplierDocumentHandler(
    ISupplierRepository supplierRepository, IFileStorage fileStorage, IMapper mapper,IDistributedCache cache)
    : IRequestHandler<AddSupplierDocumentCommand, SupplierViewModel>
{
    private static readonly string[] AllowedExtensions = [".pdf", ".doc", ".docx"];
    private static readonly string[] AllowedContentTypes = ["application/pdf", "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"];
    private const long MaxSizeBytes = 5 * 1024 * 1024; // 5 MB

    public async Task<SupplierViewModel> Handle(AddSupplierDocumentCommand request, CancellationToken ct)
    {
        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, ct)
                       ?? throw new NotFoundException($"Supplier with id '{request.SupplierId}' was not found.");

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ValidationException("Only .pdf, .doc, and .docx files are allowed.");

        if (!AllowedContentTypes.Contains(request.ContentType))
            throw new ValidationException("File content type is not a supported document type.");

        if (request.Length > MaxSizeBytes)
            throw new ValidationException("File size must not exceed 5 MB.");

        var result = await fileStorage.UploadAsync(
            "supplier-documents", supplier.SupplierId, request.Content, request.FileName, request.ContentType, ct);

        var document = SupplierDocument.Create(supplier.SupplierId, result.FileName, result.ObjectKey);
        supplier.AddDocument(document);

        await supplierRepository.UpdateAsync(supplier, ct);
        await cache.RemoveAsync(SupplierCacheKeys.ById(supplier.SupplierId), ct);
        await cache.RemoveAsync(SupplierCacheKeys.List, ct);
        
        return mapper.Map<SupplierViewModel>(supplier);
    }
}