using Infrastructure.Mqtt.Interfaces;
using Infrastructure.Mqtt.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Server.Services;
using Application.Interfaces;

namespace Server.Messaging;

/// <summary>
/// Background service that subscribes to MQTT topics and forwards messages to SignalR
/// </summary>
public class MqttToSignalRService : BackgroundService
{
    private readonly ILogger<MqttToSignalRService> _logger;
    private readonly IMqttSubscriber _mqttSubscriber;
    private readonly IMqttSignalRBridge _signalRBridge;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly MqttTopicOptions _topicOptions;

    public MqttToSignalRService(
        ILogger<MqttToSignalRService> logger,
        IMqttSubscriber mqttSubscriber,
        IMqttSignalRBridge signalRBridge,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<MqttTopicOptions> topicOptions)
    {
        _logger = logger;
        _mqttSubscriber = mqttSubscriber;
        _signalRBridge = signalRBridge;
        _serviceScopeFactory = serviceScopeFactory;
        _topicOptions = topicOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MQTT to SignalR Service is starting");

        try
        {
            // Start the MQTT subscriber
            await _mqttSubscriber.StartAsync(stoppingToken);

            // Subscribe to all meter readings (using wildcard)
            var readingsTopic = _topicOptions.GetAllReadingsTopic();
            _logger.LogInformation("Subscribing to MQTT topic: {Topic}", readingsTopic);

            await _mqttSubscriber.SubscribeAsync(readingsTopic, async (topic, payload) =>
            {
                try
                {
                    var json = System.Text.Encoding.UTF8.GetString(payload);
                    var reading = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                    if (reading != null && reading.ContainsKey("ClientId") && reading.ContainsKey("Value"))
                    {
                        var userId = reading["ClientId"].GetString();
                        var value = reading["Value"].GetSingle();
                        
                        if (!string.IsNullOrEmpty(userId))
                        {
                            _logger.LogDebug("Received meter reading for user {UserId}: {Value} kWh", userId, value);
                            
                            // Save reading to database (using scope to resolve scoped service)
                            try
                            {
                                using (var scope = _serviceScopeFactory.CreateScope())
                                {
                                    var readingService = scope.ServiceProvider.GetRequiredService<IReadingService>();
                                    await readingService.CreateAsync(userId, value);
                                    _logger.LogInformation("Saved reading to database: User={UserId}, Value={Value}", userId, value);
                                }
                            }
                            catch (Exception saveEx)
                            {
                                _logger.LogError(saveEx, "Failed to save reading to database for user {UserId}", userId);
                            }
                            
                            // Forward to SignalR
                            await _signalRBridge.BroadcastMeterReading(userId, reading);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing MQTT message from topic {Topic}", topic);
                }
            }, stoppingToken);

            _logger.LogInformation("Successfully subscribed to MQTT topics");

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                if (!_mqttSubscriber.IsConnected)
                {
                    _logger.LogWarning("MQTT Subscriber is disconnected, attempting to reconnect...");
                    try
                    {
                        await _mqttSubscriber.StartAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to reconnect MQTT subscriber");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MQTT to SignalR Service");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MQTT to SignalR Service is stopping");
        await _mqttSubscriber.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
