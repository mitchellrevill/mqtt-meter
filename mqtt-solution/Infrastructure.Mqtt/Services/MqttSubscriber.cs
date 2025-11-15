using Infrastructure.Mqtt.Configuration;
using Infrastructure.Mqtt.Interfaces;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Infrastructure.Mqtt.Services;

/// <summary>
/// MQTT Subscriber implementation with shared subscription support
/// </summary>
public class MqttSubscriber : IMqttSubscriber, IDisposable
{
    private readonly ILogger<MqttSubscriber> _logger;
    private readonly RabbitMqOptions _options;
    private IManagedMqttClient? _mqttClient;
    private readonly ConcurrentDictionary<string, List<Func<string, byte[], Task>>> _messageHandlers = new();
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;
    private bool _started;

    public MqttSubscriber(
        ILogger<MqttSubscriber> logger,
        IOptions<RabbitMqOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public bool IsConnected => _mqttClient?.IsConnected ?? false;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_started && _mqttClient?.IsConnected == true)
            {
                return;
            }

            var factory = new MqttFactory();
            _mqttClient = factory.CreateManagedMqttClient();

            // Configure MQTT client options
            var clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_options.Host, _options.Port)
                .WithClientId($"{_options.ClientIdPrefix}-subscriber-{Guid.NewGuid():N}")
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(_options.KeepAliveInterval))
                .WithTimeout(TimeSpan.FromSeconds(_options.ConnectionTimeout))
                .WithCleanSession(false); // Important for shared subscriptions

            // Only add credentials if provided
            if (!string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
            {
                clientOptions = clientOptions.WithCredentials(_options.Username, _options.Password);
            }

            if (_options.UseSsl)
            {
                clientOptions = clientOptions.WithTls();
            }

            var managedOptions = new ManagedMqttClientOptionsBuilder()
                .WithClientOptions(clientOptions.Build())
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(_options.ReconnectDelay))
                .Build();

            // Set up event handlers
            _mqttClient.ConnectedAsync += OnConnectedAsync;
            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
            _mqttClient.ConnectingFailedAsync += OnConnectingFailedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

            await _mqttClient.StartAsync(managedOptions);

            _logger.LogInformation("MQTT Subscriber connecting to {Host}:{Port}", _options.Host, _options.Port);

            // Wait for connection
            var timeout = TimeSpan.FromSeconds(_options.ConnectionTimeout);
            var startTime = DateTime.UtcNow;
            while (!_mqttClient.IsConnected && DateTime.UtcNow - startTime < timeout)
            {
                await Task.Delay(100, cancellationToken);
            }

            if (_mqttClient.IsConnected)
            {
                _logger.LogInformation("MQTT Subscriber connected successfully");
                _started = true;
            }
            else
            {
                _logger.LogWarning("MQTT Subscriber connection timeout");
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_mqttClient != null)
        {
            await _mqttClient.StopAsync();
            _started = false;
            _logger.LogInformation("MQTT Subscriber stopped");
        }
    }

    public async Task SubscribeAsync(string topic, Func<string, byte[], Task> messageHandler, CancellationToken cancellationToken = default)
    {
        if (!_started)
        {
            await StartAsync(cancellationToken);
        }

        // Add handler to dictionary
        _messageHandlers.AddOrUpdate(
            topic,
            _ => new List<Func<string, byte[], Task>> { messageHandler },
            (_, handlers) =>
            {
                handlers.Add(messageHandler);
                return handlers;
            });

        // Subscribe to topic
        var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
            .WithTopicFilter(topic, MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient!.SubscribeAsync(subscribeOptions.TopicFilters);

        _logger.LogInformation("Subscribed to topic: {Topic}", topic);
    }

    public async Task SubscribeAsync<T>(string topic, Func<string, T, Task> messageHandler, CancellationToken cancellationToken = default)
    {
        await SubscribeAsync(topic, async (t, payload) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(payload);
                var typedPayload = JsonSerializer.Deserialize<T>(json);
                if (typedPayload != null)
                {
                    await messageHandler(t, typedPayload);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing message from topic {Topic}", t);
            }
        }, cancellationToken);
    }

    public async Task SubscribeSharedAsync(string groupName, string topic, Func<string, byte[], Task> messageHandler, CancellationToken cancellationToken = default)
    {
        // RabbitMQ shared subscription format: $share/{groupName}/{topic}
        var sharedTopic = $"$share/{groupName}/{topic}";
        await SubscribeAsync(sharedTopic, messageHandler, cancellationToken);
        
        _logger.LogInformation("Subscribed to shared subscription: Group={GroupName}, Topic={Topic}", groupName, topic);
    }

    public async Task SubscribeSharedAsync<T>(string groupName, string topic, Func<string, T, Task> messageHandler, CancellationToken cancellationToken = default)
    {
        var sharedTopic = $"$share/{groupName}/{topic}";
        await SubscribeAsync<T>(sharedTopic, messageHandler, cancellationToken);
        
        _logger.LogInformation("Subscribed to shared subscription: Group={GroupName}, Topic={Topic}", groupName, topic);
    }

    public async Task UnsubscribeAsync(string topic, CancellationToken cancellationToken = default)
    {
        if (_mqttClient?.IsConnected == true)
        {
            await _mqttClient.UnsubscribeAsync(topic);
            _messageHandlers.TryRemove(topic, out _);
            _logger.LogInformation("Unsubscribed from topic: {Topic}", topic);
        }
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs args)
    {
        var topic = args.ApplicationMessage.Topic;
        var payload = args.ApplicationMessage.Payload;

        _logger.LogDebug("Message received on topic: {Topic}, Size: {Size} bytes", topic, payload.Length);

        // Find matching handlers (support wildcards)
        var matchingHandlers = _messageHandlers
            .Where(kvp => TopicMatches(kvp.Key, topic))
            .SelectMany(kvp => kvp.Value)
            .ToList();

        foreach (var handler in matchingHandlers)
        {
            try
            {
                await handler(topic, payload);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message from topic {Topic}", topic);
            }
        }
    }

    private bool TopicMatches(string filter, string topic)
    {
        // Simple wildcard matching for MQTT topics
        // + matches single level, # matches multiple levels
        
        var filterParts = filter.Split('/');
        var topicParts = topic.Split('/');

        int filterIndex = 0;
        int topicIndex = 0;

        while (filterIndex < filterParts.Length && topicIndex < topicParts.Length)
        {
            if (filterParts[filterIndex] == "#")
            {
                return true; // # matches everything remaining
            }

            if (filterParts[filterIndex] != "+" && filterParts[filterIndex] != topicParts[topicIndex])
            {
                return false;
            }

            filterIndex++;
            topicIndex++;
        }

        return filterIndex == filterParts.Length && topicIndex == topicParts.Length;
    }

    private Task OnConnectedAsync(MqttClientConnectedEventArgs args)
    {
        _logger.LogInformation("MQTT Subscriber connected to broker");
        return Task.CompletedTask;
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        _logger.LogWarning("MQTT Subscriber disconnected from broker. Reason: {Reason}", args.Reason);
        return Task.CompletedTask;
    }

    private Task OnConnectingFailedAsync(ConnectingFailedEventArgs args)
    {
        _logger.LogError(args.Exception, "MQTT Subscriber connection failed");
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _mqttClient?.Dispose();
            _connectionLock.Dispose();
            _disposed = true;
        }
    }
}
