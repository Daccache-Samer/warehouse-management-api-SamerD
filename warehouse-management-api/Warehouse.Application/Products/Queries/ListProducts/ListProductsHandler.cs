using AutoMapper;
using MediatR;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Queries.ListProducts;

public class ListProductsHandler(IProductRepository productRepository, IMapper mapper)
    : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductViewModel>>
{
    public async Task<IReadOnlyList<ProductViewModel>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        var filtered = request.OnlyAvailable
            ? products.Where(p => p is { IsArchived: false, QuantityInStock: > 0 })
            : products.AsEnumerable();

        return filtered
            .OrderByDescending(p => p.CreatedAt)
            .Select(mapper.Map<ProductViewModel>)
            .ToList();
    }
}