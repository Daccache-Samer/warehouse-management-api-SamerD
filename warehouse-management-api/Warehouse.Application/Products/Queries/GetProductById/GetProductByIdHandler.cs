using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Products.ViewModels;
using Warehouse.DomainWarehouse.Domain.Products;
using Warehouse.Application.Trackers;

namespace Warehouse.Application.Products.Queries.GetProductById;

public class GetProductByIdHandler(
    IProductRepository productRepository, IMapper mapper,IDistributedCache distributedCache,CacheStatisticsTracker cacheStatisticsTracker)
    : IRequestHandler<GetProductByIdQuery, ProductViewModel?>
{
    public async Task<ProductViewModel?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"{nameof(GetProductByIdQuery)}-{request.ProductId}";
        var cached = await distributedCache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            cacheStatisticsTracker.RecordHit();
            return JsonSerializer.Deserialize<ProductViewModel>(cached);
        }
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ??  throw new NotFoundException($"Product with ID '{request.ProductId}' not found.");
        var viewModel = mapper.Map<ProductViewModel>(product);
        await distributedCache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(viewModel),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) },
            cancellationToken);
        cacheStatisticsTracker.RecordMiss(cacheKey);
        return viewModel;
    }
}