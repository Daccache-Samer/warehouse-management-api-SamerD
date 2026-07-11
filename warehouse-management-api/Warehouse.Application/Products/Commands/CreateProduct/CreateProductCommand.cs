using MediatR;
using Warehouse.Application.Products.ViewModels;

namespace Warehouse.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string SKU,
    string Description,
    decimal Price,
    int QuantityInStock,
    DateTime ExpiryDate) : IRequest<ProductViewModel>;