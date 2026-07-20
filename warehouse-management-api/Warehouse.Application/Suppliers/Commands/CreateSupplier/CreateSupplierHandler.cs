using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Commands.CreateSupplier;

public class CreateSupplierHandler(
    ISupplierRepository supplierRepository,IMapper mapper,ILogger<CreateSupplierHandler> logger,IDistributedCache cache)
    : IRequestHandler<CreateSupplierCommand, SupplierViewModel>
{
    public async Task<SupplierViewModel> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = Supplier.Create(request.Name, request.Country, request.ContactEmail, request.PhoneNumber);

        await supplierRepository.AddAsync(supplier, cancellationToken);
        await cache.RemoveAsync("ListSuppliersHandler_ListSuppliersQuery", cancellationToken);

        logger.LogInformation(
            "Supplier created: {SupplierId} {Name} ({Country})",
            supplier.SupplierId, supplier.Name, supplier.Country);

        return mapper.Map<SupplierViewModel>(supplier);
    }
}