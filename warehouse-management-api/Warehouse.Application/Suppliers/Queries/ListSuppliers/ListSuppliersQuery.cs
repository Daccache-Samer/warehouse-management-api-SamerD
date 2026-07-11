using MediatR;
using Warehouse.Application.Suppliers.ViewModels;

namespace Warehouse.Application.Suppliers.Queries.ListSuppliers;

public record ListSuppliersQuery : IRequest<IReadOnlyList<SupplierViewModel>>;