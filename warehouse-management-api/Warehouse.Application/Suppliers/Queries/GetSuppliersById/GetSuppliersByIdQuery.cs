using MediatR;

namespace Warehouse.Application.Suppliers.Queries.GetSuppliersById;

public record GetSupplierByIdQuery(string SupplierId) : IRequest<SupplierDto?>;