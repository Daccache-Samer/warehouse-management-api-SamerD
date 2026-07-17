using AutoMapper;
using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Queries.GetSuppliersById;

public class GetSupplierByIdHandler(ISupplierRepository supplierRepository,IMapper mapper)
    : IRequestHandler<GetSupplierByIdQuery, SupplierViewModel?>
{
    public async Task<SupplierViewModel?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken)
                       ??  throw new NotFoundException($"Product with ID '{request.SupplierId}' not found.");
        return mapper.Map<SupplierViewModel>(supplier);
    }
}