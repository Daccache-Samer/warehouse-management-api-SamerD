using AutoMapper;
using MediatR;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Queries.ListSuppliers;

public class ListSuppliersHandler(ISupplierRepository supplierRepository,IMapper mapper)
    : IRequestHandler<ListSuppliersQuery, IReadOnlyList<SupplierViewModel>>
{
    public async Task<IReadOnlyList<SupplierViewModel>> Handle(ListSuppliersQuery request, CancellationToken cancellationToken)
    {
        var suppliers = await supplierRepository.GetAllAsync(cancellationToken);
        return suppliers.Select(mapper.Map<SupplierViewModel>).ToList();
    }
}