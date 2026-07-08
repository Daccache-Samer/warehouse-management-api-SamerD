using MediatR;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Commands.CreateSupplier;

public class CreateSupplierHandler(ISupplierRepository supplierRepository)
    : IRequestHandler<CreateSupplierCommand, SupplierDto>
{
    public async Task<SupplierDto> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = Supplier.Create(request.Name, request.Country, request.ContactEmail, request.PhoneNumber);

        await supplierRepository.AddAsync(supplier, cancellationToken);

        return SupplierDto.FromDomain(supplier);
    }
}