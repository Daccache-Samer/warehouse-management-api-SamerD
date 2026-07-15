using AutoMapper;
using MediatR;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Queries.GetProductById;

public class GetProductByIdHandler(IProductRepository productRepository, IMapper mapper)
    : IRequestHandler<GetProductByIdQuery, ProductViewModel?>
{
    public async Task<ProductViewModel?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ??  throw new NotFoundException($"Product with ID '{request.ProductId}' not found.");
        return mapper.Map<ProductViewModel>(product);
    }
}