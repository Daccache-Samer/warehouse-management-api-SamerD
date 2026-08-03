namespace Warehouse.DomainWarehouse.Domain.Common;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent integrationEvent, string routingKey, CancellationToken ct = default)
        where TEvent : class;
}