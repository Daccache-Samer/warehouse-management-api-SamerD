using MediatR;

namespace Warehouse.Application.Suppliers.Commands.DeactivateSupplier;

public record DeactivateSupplierCommand(string SupplierId) : IRequest;