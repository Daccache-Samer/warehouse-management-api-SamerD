using MediatR;

namespace Warehouse.Application.Products.Commands.AssignSupplierToProduct;

public abstract record AssignSupplierToProductCommand(string ProductId, string SupplierId) : IRequest<ProductDto>;