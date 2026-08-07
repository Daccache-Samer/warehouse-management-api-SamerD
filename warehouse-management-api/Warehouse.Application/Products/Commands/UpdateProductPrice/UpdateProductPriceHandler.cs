using AutoMapper;
using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.UpdateProductPrice;

public class UpdateProductPriceHandler(IProductRepository productRepository, IMapper mapper)
    : IRequestHandler<UpdateProductPriceCommand, ProductViewModel>
{
    public async Task<ProductViewModel> Handle(UpdateProductPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id '{request.ProductId}' was not found.");

        product.UpdatePrice(request.Price);

        await productRepository.UpdateAsync(product, cancellationToken);

        return mapper.Map<ProductViewModel>(product);
    }
}