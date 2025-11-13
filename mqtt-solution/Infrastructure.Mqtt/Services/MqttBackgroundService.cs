using Infrastructure.Mqtt.Configuration;
using Infrastructure.Mqtt.Interfaces;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.IO;

namespace Infrastructure.Mqtt.Services;

/// <summary>
/// Background service to manage MQTT subscriber lifecycle
/// </summary>
public class MqttBackgroundService : BackgroundService
{
    private readonly ILogger<MqttBackgroundService> _logger;
    private readonly IMqttSubscriber _subscriber;
    private readonly MqttTopicOptions _topicOptions;
    private readonly RabbitMqOptions _rabbitMqOptions;

    public MqttBackgroundService(
        ILogger<MqttBackgroundService> logger,
        IMqttSubscriber subscriber,
        IOptions<MqttTopicOptions> topicOptions,
        IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _logger = logger;
        _subscriber = subscriber;
        _topicOptions = topicOptions.Value;
        _rabbitMqOptions = rabbitMqOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MQTT Background Service is starting");

        try
        {
            // Try to start subscriber but don't fail if it can't connect
            try
            {
                await _subscriber.StartAsync(stoppingToken);
                
                // Subscribe to meter readings topic
                await _subscriber.SubscribeAsync(_topicOptions.GetAllReadingsTopic(), async (topic, payload) =>
                {
                    try
                    {
                        var json = System.Text.Encoding.UTF8.GetString(payload);
                        var message = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                        var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - Topic: {topic} - Payload: {json}{Environment.NewLine}";
                        await File.AppendAllTextAsync("readings.log", logEntry, stoppingToken);
                        _logger.LogInformation("Logged reading from topic {Topic}", topic);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to log reading from topic {Topic}", topic);
                    }
                }, stoppingToken);
                
                _logger.LogInformation("Subscribed to readings topic: {Topic}", _topicOptions.GetAllReadingsTopic());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to start MQTT Subscriber. Publisher-only mode will be used. Error: {Message}", ex.Message);
            }

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                if (!_subscriber.IsConnected)
                {
                    _logger.LogDebug("MQTT Subscriber is disconnected");
                    // Optionally try to reconnect
                    try
                    {
                        await _subscriber.StartAsync(stoppingToken);
                    }
                    catch
                    {
                        // Silently fail, already logged warnings above
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MQTT Background Service");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MQTT Background Service is stopping");
        await _subscriber.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
