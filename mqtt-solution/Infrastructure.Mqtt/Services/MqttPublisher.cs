using Infrastructure.Mqtt.Configuration;
using Infrastructure.Mqtt.Interfaces;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System.Text.Json;

namespace Infrastructure.Mqtt.Services;

/// <summary>
/// MQTT Publisher implementation
/// </summary>
public class MqttPublisher : IMqttPublisher, IDisposable
{
    private readonly ILogger<MqttPublisher> _logger;
    private readonly RabbitMqOptions _options;
    private IManagedMqttClient? _mqttClient;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;

    public MqttPublisher(
        ILogger<MqttPublisher> logger,
        IOptions<RabbitMqOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public bool IsConnected => _mqttClient?.IsConnected ?? false;

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_mqttClient?.IsConnected == true)
            {
                return true;
            }

            var factory = new MqttFactory();
            _mqttClient = factory.CreateManagedMqttClient();

            // Configure MQTT client options
            var clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(_options.Host, _options.Port)
                .WithClientId($"{_options.ClientIdPrefix}-publisher-{Guid.NewGuid():N}")
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(_options.KeepAliveInterval))
                .WithTimeout(TimeSpan.FromSeconds(_options.ConnectionTimeout))
                .WithCleanSession(true);

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

            await _mqttClient.StartAsync(managedOptions);

            _logger.LogInformation("MQTT Publisher connecting to {Host}:{Port}", _options.Host, _options.Port);

            // Wait for connection with timeout
            var timeout = TimeSpan.FromSeconds(_options.ConnectionTimeout);
            var startTime = DateTime.UtcNow;
            while (!_mqttClient.IsConnected && DateTime.UtcNow - startTime < timeout)
            {
                await Task.Delay(100, cancellationToken);
            }

            if (_mqttClient.IsConnected)
            {
                _logger.LogInformation("MQTT Publisher connected successfully");
                return true;
            }

            _logger.LogWarning("MQTT Publisher connection timeout");
            return false;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_mqttClient != null)
        {
            await _mqttClient.StopAsync();
            _logger.LogInformation("MQTT Publisher disconnected");
        }
    }

    public async Task PublishAsync<T>(string topic, T payload, bool retain = false, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(payload);
        var payloadBytes = Encoding.UTF8.GetBytes(json);
        await PublishAsync(topic, payloadBytes, retain, cancellationToken);
    }

    public async Task PublishAsync(string topic, byte[] payload, bool retain = false, CancellationToken cancellationToken = default)
    {
        if (_mqttClient == null || !_mqttClient.IsConnected)
        {
            var connected = await ConnectAsync(cancellationToken);
            if (!connected)
            {
                throw new InvalidOperationException("Failed to connect to MQTT broker");
            }
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .WithRetainFlag(retain)
            .Build();

        await _mqttClient!.EnqueueAsync(message);

        _logger.LogDebug("Published message to topic: {Topic}, Size: {Size} bytes", topic, payload.Length);
    }

    private Task OnConnectedAsync(MqttClientConnectedEventArgs args)
    {
        _logger.LogInformation("MQTT Publisher connected to broker");
        return Task.CompletedTask;
    }

    private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs args)
    {
        _logger.LogWarning("MQTT Publisher disconnected from broker. Reason: {Reason}", args.Reason);
        return Task.CompletedTask;
    }

    private Task OnConnectingFailedAsync(ConnectingFailedEventArgs args)
    {
        _logger.LogError(args.Exception, "MQTT Publisher connection failed");
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
