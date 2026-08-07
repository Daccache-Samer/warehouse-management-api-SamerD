using Warehouse.DomainWarehouse.Domain.Common;

namespace Warehouse.Api.IntegrationTests.TestUtilities.Helpers;

public sealed class NoOpEventPublisher : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent integrationEvent, string routingKey, CancellationToken ct = default)
        where TEvent : class
        => Task.CompletedTask;
}