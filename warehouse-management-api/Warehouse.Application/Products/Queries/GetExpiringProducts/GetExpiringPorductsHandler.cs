using AutoMapper;
using MediatR;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Queries.GetExpiringProducts;

public class GetExpiringProductsHandler(IProductRepository productRepository, IMapper mapper)
    : IRequestHandler<GetExpiringProductsQuery, IReadOnlyList<ExpiringProductViewModel>>
{
    public async Task<IReadOnlyList<ExpiringProductViewModel>> Handle(
        GetExpiringProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await productRepository.GetExpiringProductsAsync(request.WithinDays, cancellationToken);

        return mapper.Map<IReadOnlyList<ExpiringProductViewModel>>(products);
    }
}