namespace Infrastructure.Mqtt.Interfaces;

/// <summary>
/// Interface for subscribing to MQTT messages
/// </summary>
public interface IMqttSubscriber
{
    /// <summary>
    /// Subscribe to a topic with a message handler
    /// </summary>
    Task SubscribeAsync(string topic, Func<string, byte[], Task> messageHandler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe to a topic with a typed message handler
    /// </summary>
    Task SubscribeAsync<T>(string topic, Func<string, T, Task> messageHandler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe to a shared subscription
    /// </summary>
    Task SubscribeSharedAsync(string groupName, string topic, Func<string, byte[], Task> messageHandler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe to a shared subscription with typed handler
    /// </summary>
    Task SubscribeSharedAsync<T>(string groupName, string topic, Func<string, T, Task> messageHandler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsubscribe from a topic
    /// </summary>
    Task UnsubscribeAsync(string topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the subscriber is connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Start the subscriber
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop the subscriber
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
}
