using MediatR;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Queries.GetSuppliersById;

public class GetSupplierByIdHandler(ISupplierRepository supplierRepository)
    : IRequestHandler<GetSupplierByIdQuery, SupplierDto?>
{
    public async Task<SupplierDto?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken);
        return supplier is null ? null : SupplierDto.FromDomain(supplier);
    }
}