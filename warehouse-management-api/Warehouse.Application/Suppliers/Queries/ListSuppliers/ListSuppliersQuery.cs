using MediatR;

namespace Warehouse.Application.Suppliers.Queries.ListSuppliers;

public record ListSuppliersQuery : IRequest<IReadOnlyList<SupplierDto>>;