using System.Text.Json;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Warehouse.Application.Exceptions;
using Warehouse.Application.Suppliers.ViewModels;
using Warehouse.DomainWarehouse.Domain.Suppliers;

namespace Warehouse.Application.Suppliers.Queries.GetSuppliersById;

public class GetSupplierByIdHandler(ISupplierRepository supplierRepository,IMapper mapper,IDistributedCache cache)
    : IRequestHandler<GetSupplierByIdQuery, SupplierViewModel?>
{
    public async Task<SupplierViewModel?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"{nameof(GetSupplierByIdQuery)}-{request.SupplierId}";
        var cached =  await cache.GetStringAsync(cacheKey,cancellationToken);
        if (cached is not null)
        {
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

        return viewModel;
    }
}