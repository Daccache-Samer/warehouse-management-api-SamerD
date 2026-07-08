using MediatR;

namespace Warehouse.Application.Suppliers.Queries.ListSuppliers;

public abstract record ListSuppliersQuery : IRequest<IReadOnlyList<SupplierDto>>;