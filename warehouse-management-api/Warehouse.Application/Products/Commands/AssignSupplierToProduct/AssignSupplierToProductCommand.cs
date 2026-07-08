using MediatR;

namespace Warehouse.Application.Products.Commands.AssignSupplierToProduct;

public record AssignSupplierToProductCommand(string ProductId, string SupplierId) : IRequest<ProductDto>;