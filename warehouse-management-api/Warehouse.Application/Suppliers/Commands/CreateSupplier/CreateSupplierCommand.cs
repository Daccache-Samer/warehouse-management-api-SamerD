using MediatR;

namespace Warehouse.Application.Suppliers.Commands.CreateSupplier;

public abstract record CreateSupplierCommand(
    string Name,
    string Country,
    string ContactEmail,
    string PhoneNumber) : IRequest<SupplierDto>;