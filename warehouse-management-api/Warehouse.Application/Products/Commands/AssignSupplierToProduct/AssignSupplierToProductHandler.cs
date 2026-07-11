using AutoMapper;
using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Products.Commands.AssignSupplierToProduct;

public class AssignSupplierToProductHandler(
    IProductRepository productRepository,ISupplierRepository supplierRepository,IMapper mapper)
    : IRequestHandler<AssignSupplierToProductCommand, ProductViewModel>
{
    public async Task<ProductViewModel> Handle(AssignSupplierToProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken)
                       ?? throw new NotFoundException($"Supplier with id '{request.SupplierId}' was not found.");

        product.AssignSupplier(supplier); // throws DomainException if supplier.IsActive == false, or product archived

        await productRepository.UpdateAsync(product, cancellationToken);

        return mapper.Map<ProductViewModel>(product);
    }
}