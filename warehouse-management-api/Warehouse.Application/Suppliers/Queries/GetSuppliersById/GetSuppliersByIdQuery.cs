using MediatR;

namespace Warehouse.Application.Suppliers.Queries.GetSuppliersById;

public abstract record GetSupplierByIdQuery(string SupplierId) : IRequest<SupplierDto?>;