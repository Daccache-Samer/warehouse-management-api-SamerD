using MediatR;
using Microsoft.Extensions.Logging;
using Warehouse.Application.Exceptions;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Commands.DeactivateSupplier;

public class DeactivateSupplierHandler(ISupplierRepository supplierRepository,ILogger<DeactivateSupplierHandler> logger)
    : IRequestHandler<DeactivateSupplierCommand>
{
    public async Task Handle(DeactivateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken)
                       ?? throw new NotFoundException($"Supplier with id '{request.SupplierId}' was not found.");

        supplier.Deactivate();

        await supplierRepository.UpdateAsync(supplier, cancellationToken);
        logger.LogInformation("Supplier deactivated: {SupplierId} {Name}", supplier.SupplierId, supplier.Name);
    }
}