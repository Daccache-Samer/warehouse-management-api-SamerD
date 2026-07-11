using MediatR;
using Warehouse.Application.Suppliers.ViewModels;

namespace Warehouse.Application.Suppliers.Queries.GetSuppliersById;

public record GetSupplierByIdQuery(string SupplierId) : IRequest<SupplierViewModel?>;