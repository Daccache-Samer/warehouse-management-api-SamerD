using AutoMapper;
using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.UpdateProductQuantity;

public class UpdateProductQuantityHandler(IProductRepository productRepository,IMapper mapper)
    : IRequestHandler<UpdateProductQuantityCommand, ProductViewModel>
{
    public async Task<ProductViewModel> Handle(UpdateProductQuantityCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        product.UpdateQuantity(request.QuantityInStock);

        await productRepository.UpdateAsync(product, cancellationToken);

        return mapper.Map<ProductViewModel>(product);
    }
}