using MediatR;
using Warehouse.Application.Suppliers.ViewModels;

namespace Warehouse.Application.Suppliers.Commands.CreateSupplier;

public record CreateSupplierCommand(
    string Name,
    string Country,
    string ContactEmail,
    string PhoneNumber) : IRequest<SupplierViewModel>;