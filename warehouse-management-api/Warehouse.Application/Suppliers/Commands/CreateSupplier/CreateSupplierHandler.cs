using AutoMapper;
using MediatR;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Commands.CreateSupplier;

public class CreateSupplierHandler(ISupplierRepository supplierRepository,IMapper mapper)
    : IRequestHandler<CreateSupplierCommand, SupplierViewModel>
{
    public async Task<SupplierViewModel> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = Supplier.Create(request.Name, request.Country, request.ContactEmail, request.PhoneNumber);

        await supplierRepository.AddAsync(supplier, cancellationToken);

        return mapper.Map<SupplierViewModel>(supplier);
    }
}