using MediatR;

namespace Warehouse.Application.Suppliers.Commands.DeactivateSupplier;

public abstract record DeactivateSupplierCommand(string SupplierId) : IRequest;