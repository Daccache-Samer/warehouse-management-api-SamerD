using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Queries.ListSuppliers;

public class ListSuppliersHandler(ISupplierRepository supplierRepository,IMapper mapper,IDistributedCache cache)
    : IRequestHandler<ListSuppliersQuery, IReadOnlyList<SupplierViewModel>>
{
    public async Task<IReadOnlyList<SupplierViewModel>> Handle(ListSuppliersQuery request, CancellationToken cancellationToken)
    {
        const string cacheKey = $"{nameof(ListSuppliersHandler)}_{nameof(ListSuppliersQuery)}";
        var  cached = await cache.GetStringAsync(cacheKey,cancellationToken);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<List<SupplierViewModel>>(cached) ?? throw new InvalidOperationException();
        }
        var suppliers = await supplierRepository.GetAllAsync(cancellationToken);
        var result = suppliers.Select(mapper.Map<SupplierViewModel>).ToList();

        await cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2) },
            cancellationToken);

        return result;
    }
}