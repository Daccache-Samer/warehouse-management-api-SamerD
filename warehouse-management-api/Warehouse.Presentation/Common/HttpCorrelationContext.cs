using Warehouse.DomainWarehouse.Domain.Common;

namespace warehouse_management_api.Common;

public class HttpCorrelationContext(IHttpContextAccessor httpContextAccessor) : ICorrelationContext
{
    public string CorrelationId => httpContextAccessor.HttpContext?.TraceIdentifier ?? string.Empty;
}