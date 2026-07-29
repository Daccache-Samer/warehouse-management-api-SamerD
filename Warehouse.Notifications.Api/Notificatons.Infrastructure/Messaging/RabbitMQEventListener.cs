using System.Text;
using Microsoft.Extensions.Options;
using Notifications.Application.Common;
using Notifications.Application.Notification.Commands.ProcessWarehouseEvent;
using Notifications.Application.IntegrationEvents;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notifications.Infrastructure.Messaging;

public class RabbitMqEventListener(
    IOptions<RabbitMqSettings> settings, IServiceScopeFactory scopeFactory, ILogger<RabbitMqEventListener> logger)
    : BackgroundService
{
    private readonly RabbitMqSettings _settings = settings.Value;

    private IConnection? _connection;
    private IChannel? _channel;

    private static readonly string[] RoutingKeys =
    [
        WarehouseEventTypes.StockLow, WarehouseEventTypes.StockAdjusted,
        WarehouseEventTypes.ProductCreated, WarehouseEventTypes.FileUploaded
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);
        if (_channel is null) return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, ea) => await HandleMessageAsync(ea);

        await _channel.BasicConsumeAsync(_settings.QueueName, autoAck: false, consumer, cancellationToken: stoppingToken);

        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) { /* normal shutdown */ }
    }

    private async Task ConnectWithRetryAsync(CancellationToken stoppingToken)
    {
        var delay = TimeSpan.FromSeconds(5);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    ClientProvidedName = "warehouse-notifications-consumer"
                };

                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await _channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: stoppingToken);

                // Create Dead Letter queue
                var dlxName = _settings.ExchangeName + ".dlx";
                var dlqName = _settings.QueueName + ".dlq";

                await _channel.ExchangeDeclareAsync(dlxName, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: stoppingToken);
                await _channel.QueueDeclareAsync(dlqName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
                await _channel.QueueBindAsync(dlqName, dlxName, routingKey: "", cancellationToken: stoppingToken);
                
                var mainQueueArgs = new Dictionary<string, object?>
                {
                    { "x-dead-letter-exchange", dlxName }
                };
                await _channel.QueueDeclareAsync(_settings.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: mainQueueArgs, cancellationToken: stoppingToken);

                foreach (var routingKey in RoutingKeys)
                    await _channel.QueueBindAsync(_settings.QueueName, _settings.ExchangeName, routingKey, cancellationToken: stoppingToken);

                await _channel.BasicQosAsync(0, 1, false, stoppingToken);
                logger.LogInformation("Connected to RabbitMQ, consuming {Queue}", _settings.QueueName);
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "RabbitMQ not reachable yet, retrying in {Delay}s", delay.TotalSeconds);
                try { await Task.Delay(delay, stoppingToken); } catch (TaskCanceledException) { return; }
            }
        }
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea)
    {
        var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
        Exception? lastException = null;

        for (var attempt = 1; attempt <= _settings.MaxRetryAttempts; attempt++)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var consumer = scope.ServiceProvider.GetRequiredService<IWarehouseEventConsumer>();

                var result = await consumer.HandleAsync(ea.RoutingKey, payload);
                if (result == EventProcessingResult.Duplicate)
                    logger.LogInformation("Ignored duplicate delivery on routing key {RoutingKey}", ea.RoutingKey);

                // Success: acknowledge and exit
                if (_channel != null) await _channel.BasicAckAsync(ea.DeliveryTag, false);
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                logger.LogWarning(ex,
                    "Error processing message on routing key {RoutingKey} (attempt {Attempt}/{Max})",
                    ea.RoutingKey, attempt, _settings.MaxRetryAttempts);

                if (attempt < _settings.MaxRetryAttempts)
                {
                    // Exponential backoff
                    var delay = TimeSpan.FromSeconds(_settings.RetryDelayBaseSeconds * Math.Pow(2, attempt - 1));
                    await Task.Delay(delay);
                }
            }
        }

        // All retries exhausted: send to Dead Letter Queue
        logger.LogError(lastException,
            "Message on routing key {RoutingKey} failed after {Max} attempts. Routing to DLQ.",
            ea.RoutingKey, _settings.MaxRetryAttempts);

        if (_channel != null) await _channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null) await _channel.CloseAsync(cancellationToken: cancellationToken);
        if (_connection is not null) await _connection.CloseAsync(cancellationToken: cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}