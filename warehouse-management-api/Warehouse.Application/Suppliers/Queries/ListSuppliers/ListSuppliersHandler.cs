using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.DomainWarehouse.Domain.Suppliers;
using Warehouse.Application.Trackers;

namespace Warehouse.Application.Suppliers.Queries.ListSuppliers;

public class ListSuppliersHandler(
    ISupplierRepository supplierRepository,IMapper mapper,IDistributedCache cache,CacheStatisticsTracker cacheStatisticsTracker)
    : IRequestHandler<ListSuppliersQuery, IReadOnlyList<SupplierViewModel>>
{
    public async Task<IReadOnlyList<SupplierViewModel>> Handle(ListSuppliersQuery request, CancellationToken cancellationToken)
    {
        const string cacheKey = SupplierCacheKeys.List;
        var  cached = await cache.GetStringAsync(cacheKey,cancellationToken);
        if (cached is not null)
        {
            cacheStatisticsTracker.RecordHit();
            return JsonSerializer.Deserialize<List<SupplierViewModel>>(cached) ?? throw new InvalidOperationException();
        }
        var suppliers = await supplierRepository.GetAllAsync(cancellationToken);
        var result = suppliers.Select(mapper.Map<SupplierViewModel>).ToList();

        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) },
            cancellationToken);
        
        cacheStatisticsTracker.RecordMiss(cacheKey);
        return result;
    }
}