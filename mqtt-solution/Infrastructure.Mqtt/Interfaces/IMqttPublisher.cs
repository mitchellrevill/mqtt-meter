namespace Infrastructure.Mqtt.Interfaces;

/// <summary>
/// Interface for publishing MQTT messages
/// </summary>
public interface IMqttPublisher
{
    /// <summary>
    /// Publish a message to a topic
    /// </summary>
    Task PublishAsync<T>(string topic, T payload, bool retain = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a raw message to a topic
    /// </summary>
    Task PublishAsync(string topic, byte[] payload, bool retain = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the publisher is connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Get connection status
    /// </summary>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from the broker
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}
