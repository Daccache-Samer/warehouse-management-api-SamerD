using MediatR;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Queries.ListSuppliers;

public class ListSuppliersHandler(ISupplierRepository supplierRepository)
    : IRequestHandler<ListSuppliersQuery, IReadOnlyList<SupplierDto>>
{
    public async Task<IReadOnlyList<SupplierDto>> Handle(ListSuppliersQuery request, CancellationToken cancellationToken)
    {
        var suppliers = await supplierRepository.GetAllAsync(cancellationToken);
        return suppliers.Select(SupplierDto.FromDomain).ToList();
    }
}