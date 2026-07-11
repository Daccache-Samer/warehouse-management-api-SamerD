using MediatR;
using Warehouse.Application.Products.ViewModels;

namespace Warehouse.Application.Products.Commands.AssignSupplierToProduct;

public record AssignSupplierToProductCommand(string ProductId, string SupplierId) : IRequest<ProductViewModel>;