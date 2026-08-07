using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Warehouse.Application.Products;

namespace Warehouse.Application.Common;

public class ProductCacheInvalidationBehavior<TRequest, TResponse>(IDistributedCache cache)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull, IInvalidatesProductCache
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        // NOTE: these string literals intentionally do not match ProductCacheKeys.ById/List
        // (which ignore their parameters). This is a known, pre-existing bug — invalidation
        // is currently a no-op. Preserved as-is per decision; see ProductCacheKeys.cs.
        if (request.ProductId is not null)
        {
            await cache.RemoveAsync($"GetProductByIdQuery-{request.ProductId}", cancellationToken);
        }
        await cache.RemoveAsync("ListProductsHandler_ListProductsQuery", cancellationToken);

        return response;
    }
}