using MediatR;

namespace Warehouse.Application.Suppliers.Commands.DeleteSupplierDocument;

public record DeleteSupplierDocumentCommand(string SupplierId, string DocumentId) : IRequest;