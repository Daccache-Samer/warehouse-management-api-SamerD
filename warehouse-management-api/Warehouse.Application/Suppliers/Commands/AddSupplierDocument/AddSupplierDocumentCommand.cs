using MediatR;
using Warehouse.Application.Suppliers.ViewModels;

namespace Warehouse.Application.Suppliers.Commands.AddSupplierDocument;

public record AddSupplierDocumentCommand(
    string SupplierId, Stream Content, string FileName, long Length, string ContentType)
    : IRequest<SupplierViewModel>;