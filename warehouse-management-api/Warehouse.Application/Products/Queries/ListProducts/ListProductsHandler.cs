using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;

namespace Warehouse.Application.Products.Queries.ListProducts;

public class ListProductsHandler(IProductRepository productRepository, IMapper mapper,IDistributedCache cache)
    : IRequestHandler<ListProductsQuery, IReadOnlyList<ProductViewModel>>
{
    public async Task<IReadOnlyList<ProductViewModel>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        const string cacheKey = $"{nameof(ListProductsHandler)}_{nameof(ListProductsQuery)}";
        var cached = await cache.GetStringAsync(cacheKey,cancellationToken);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<List<ProductViewModel>>(cached) ?? throw new InvalidOperationException();
        }
        var products = await productRepository.GetAllAsync(cancellationToken);

        var filtered = request.OnlyAvailable
            ? products.Where(p => p is { IsArchived: false, QuantityInStock: > 0 })
            : products.AsEnumerable();
        var result = filtered
            .OrderByDescending(p => p.CreatedAt)
            .Select(mapper.Map<ProductViewModel>)
            .ToList();
        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) },
            cancellationToken);

        return result;
    }
}