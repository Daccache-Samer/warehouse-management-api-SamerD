using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Warehouse.DomainWarehouse.Domain.Common;

namespace Warehouse.Infrastructure.Messaging;

public sealed class RabbitMqEventPublisher(IOptions<RabbitMqSettings> settings, ILogger<RabbitMqEventPublisher> logger)
    : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqSettings _settings = settings.Value;
   

    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task PublishAsync<TEvent>(TEvent integrationEvent, string routingKey, CancellationToken ct = default)
        where TEvent : class
    {
        try
        {
            var channel = await GetChannelAsync();

            var body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent);
            var props = new BasicProperties { ContentType = "application/json", DeliveryMode = (DeliveryModes)2 };

            await channel.BasicPublishAsync(_settings.ExchangeName, routingKey, mandatory: false, basicProperties: props, body: body, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish {EventType} with routing key {RoutingKey} to RabbitMQ",
                typeof(TEvent).Name, routingKey);
        }
    }

    private async Task<IChannel> GetChannelAsync()
    {
        if (_channel is { IsOpen: true }) return _channel;

        await _connectLock.WaitAsync();
        try
        {
            if (_channel is { IsOpen: true }) return _channel;

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName, Port = _settings.Port, UserName = _settings.UserName, Password = _settings.Password,
                VirtualHost = _settings.VirtualHost, AutomaticRecoveryEnabled = true,
                ClientProvidedName = "warehouse-api-publisher"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false);

            return _channel;
        }
        finally { _connectLock.Release(); }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.CloseAsync();
        if (_connection is not null) await _connection.CloseAsync();
    }
}