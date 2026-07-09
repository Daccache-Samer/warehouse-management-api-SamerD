using MediatR;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Queries.ListProducts;

public class ListProductsHandler(IProductRepository productRepository)
    : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductDto>>
{
    public async Task<IReadOnlyList<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        var filtered = request.OnlyAvailable
            ? products.Where(p => !p.IsArchived && p.QuantityInStock > 0)
            : products.AsEnumerable();

        return filtered
            .OrderByDescending(p => p.CreatedAt)
            .Select(ProductDto.FromDomain)
            .ToList();
    }
}