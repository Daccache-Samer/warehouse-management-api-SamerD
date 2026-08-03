namespace Warehouse.DomainWarehouse.Domain.Common;

public interface ICorrelationContext // added this because Warehouse.Application can't reference HttpContext directly.
{
    string CorrelationId { get; }
}