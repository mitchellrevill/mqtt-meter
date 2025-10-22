namespace Infrastructure.Mqtt.Configuration;

/// <summary>
/// Configuration options for RabbitMQ connection
/// </summary>
public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    /// <summary>
    /// RabbitMQ host address
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ port (default MQTT port is 1883)
    /// </summary>
    public int Port { get; set; } = 1883;

    /// <summary>
    /// AMQP port for management (default is 5672)
    /// </summary>
    public int AmqpPort { get; set; } = 5672;

    /// <summary>
    /// Username for authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password for authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Virtual host (default is /)
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Enable SSL/TLS
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Client ID prefix
    /// </summary>
    public string ClientIdPrefix { get; set; } = "mqtt-meter";

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Keep alive interval in seconds
    /// </summary>
    public int KeepAliveInterval { get; set; } = 60;

    /// <summary>
    /// Reconnect delay in seconds
    /// </summary>
    public int ReconnectDelay { get; set; } = 5;

    /// <summary>
    /// Enable shared subscriptions
    /// </summary>
    public bool EnableSharedSubscriptions { get; set; } = true;

    /// <summary>
    /// Shared subscription group name
    /// </summary>
    public string SharedSubscriptionGroup { get; set; } = "mqtt-meter-group";
}
