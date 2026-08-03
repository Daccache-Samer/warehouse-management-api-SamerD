using System.ComponentModel.DataAnnotations;

namespace Warehouse.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    [Required] public string HostName { get; init; } = null!;
    [Range(1, 65535)] public int Port { get; init; }
    [Required] public string UserName { get; init; } = null!;
    [Required] public string Password { get; init; } = null!;
    [Required] public string VirtualHost { get; init; } = null!;
    [Required] public string ExchangeName { get; init; } = null!;
}