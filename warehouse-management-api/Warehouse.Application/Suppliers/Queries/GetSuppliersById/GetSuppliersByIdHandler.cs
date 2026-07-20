using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.DomainWarehouse.Domain.Suppliers;
using Warehouse.Application.Trackers;

namespace Warehouse.Application.Suppliers.Queries.GetSuppliersById;

public class GetSupplierByIdHandler(
    ISupplierRepository supplierRepository,IMapper mapper,IDistributedCache cache,CacheStatisticsTracker cacheStatisticsTracker)
    : IRequestHandler<GetSupplierByIdQuery, SupplierViewModel?>
{
    public async Task<SupplierViewModel?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = SupplierCacheKeys.ById(request.SupplierId);
        var cached =  await cache.GetStringAsync(cacheKey,cancellationToken);
        if (cached is not null)
        {
            cacheStatisticsTracker.RecordHit();
            return JsonSerializer.Deserialize<SupplierViewModel>(cached);
        }
        
        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken)
                       ??  throw new NotFoundException($"Product with ID '{request.SupplierId}' not found.");
        
        var viewModel = mapper.Map<SupplierViewModel>(supplier);

        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(viewModel),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) },
            cancellationToken);
        
        cacheStatisticsTracker.RecordMiss(cacheKey);
        return viewModel;
    }
}