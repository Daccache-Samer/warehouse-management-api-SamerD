using AutoMapper;
using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Commands.CreateProduct;

public class CreateProductHandler(IProductRepository productRepository, IMapper mapper)
    : IRequestHandler<CreateProductCommand, ProductViewModel>
{
    public async Task<ProductViewModel> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existing = await productRepository.GetBySkuAsync(request.SKU, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException($"A product with SKU '{request.SKU}' already exists.");
        }

        var product = Product.Create(
            request.Name,
            request.SKU,
            request.Description,
            request.Price,
            request.QuantityInStock,
            request.ExpiryDate);

        await productRepository.AddAsync(product, cancellationToken);

        return mapper.Map<ProductViewModel>(product);
    }
}