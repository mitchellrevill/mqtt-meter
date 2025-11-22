using Infrastructure.Mqtt.Interfaces;
using Infrastructure.Mqtt.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Application.Interfaces;
using System.Linq;

namespace Server.Messaging;

/// <summary>
/// Background service that subscribes to MQTT topics and processes meter readings
/// </summary>
public class MqttReadingProcessorService : BackgroundService
{
    private readonly ILogger<MqttReadingProcessorService> _logger;
    private readonly IMqttSubscriber _mqttSubscriber;
    private readonly IMqttPublisher _mqttPublisher;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly MqttTopicOptions _topicOptions;

    public MqttReadingProcessorService(
        ILogger<MqttReadingProcessorService> logger,
        IMqttSubscriber mqttSubscriber,
        IMqttPublisher mqttPublisher,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<MqttTopicOptions> topicOptions)
    {
        _logger = logger;
        _mqttSubscriber = mqttSubscriber;
        _serviceScopeFactory = serviceScopeFactory;
        _mqttPublisher = mqttPublisher;
        _topicOptions = topicOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MQTT Reading Processor Service is starting");

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

                    if (reading != null)
                    {
                        var userId = TryGetString(reading, "ClientId") ?? TryGetString(reading, "clientId") ?? TryGetString(reading, "userId");
                        var value = TryGetDouble(reading, "Value") ?? TryGetDouble(reading, "value");
                        
                        if (!string.IsNullOrEmpty(userId) && value.HasValue)
                        {
                            _logger.LogDebug("Received meter reading for user {UserId}: {Value} kWh", userId, value);
                            
                            // Save reading to database (using scope to resolve scoped service)
                            try
                            {
                                using (var scope = _serviceScopeFactory.CreateScope())
                                {
                                    var readingService = scope.ServiceProvider.GetRequiredService<IReadingService>();
                                    await readingService.CreateAsync(userId, (float)value);
                                    _logger.LogInformation("Saved reading to database: User={UserId}, Value={Value}", userId, value);

                                    await PublishBillingSnapshotAsync(readingService, userId);
                                }
                            }
                            catch (Exception saveEx)
                            {
                                _logger.LogError(saveEx, "Failed to save reading to database for user {UserId}", userId);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing MQTT message from topic {Topic}", topic);
                }
            }, stoppingToken);

            _logger.LogInformation("Successfully subscribed to MQTT topics");

            // Subscribe to billing commands (e.g., reset). We subscribe to the billing base wildcard
            // and check for /reset commands for users.
            var billingCommandsTopic = $"{_topicOptions.BillingBaseTopic}/#";
            _logger.LogInformation("Subscribing to MQTT billing commands topic: {Topic}", billingCommandsTopic);

            await _mqttSubscriber.SubscribeAsync(billingCommandsTopic, async (topic, payload) =>
            {
                try
                {
                    // Expect topics like: {BillingBase}/{userId}/reset
                    var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && parts[^1].Equals("reset", StringComparison.OrdinalIgnoreCase))
                    {
                        // userId is the second-last segment (e.g. meters/billing/{userId}/reset)
                        var userId = parts[parts.Length - 2];

                        _logger.LogInformation("Received billing reset command for user {UserId} via MQTT", userId);

                        // Reset readings using application service in a scope
                        try
                        {
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                var readingService = scope.ServiceProvider.GetRequiredService<IReadingService>();
                                await readingService.ResetForUserAsync(userId);
                                _logger.LogInformation("Reset readings for user {UserId}", userId);

                                // Publish a billing update reflecting the reset (totals zero)
                                await PublishBillingSnapshotAsync(readingService, userId, forceZero: true);
                            }
                        }
                        catch (Exception resetEx)
                        {
                            _logger.LogError(resetEx, "Error resetting billing for user {UserId}", userId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing billing command MQTT message from topic {Topic}", topic);
                }

            }, stoppingToken);

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
            _logger.LogError(ex, "Error in MQTT Reading Processor Service");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MQTT Reading Processor Service is stopping");
        await _mqttSubscriber.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    private static string? TryGetString(Dictionary<string, JsonElement> payload, string key)
    {
        if (payload.TryGetValue(key, out var element))
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }

            return element.ToString();
        }

        return null;
    }

    private static double? TryGetDouble(Dictionary<string, JsonElement> payload, string key)
    {
        if (!payload.TryGetValue(key, out var element))
        {
            return null;
        }

        try
        {
            return element.ValueKind switch
            {
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.String when double.TryParse(element.GetString(), out var parsed) => parsed,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private async Task PublishBillingSnapshotAsync(IReadingService readingService, string userId, bool forceZero = false)
    {
        try
        {
            const double ratePerKwh = 0.15;
            double totalKwhUsed = 0.0;
            int readingCount = 0;

            if (!forceZero)
            {
                var readings = await readingService.GetByUserId(userId) ?? Enumerable.Empty<Domain.Entities.Reading>();
                var readingList = readings.ToList();
                readingCount = readingList.Count;
                totalKwhUsed = readingList.Sum(r => r.Value);
            }

            var billingPayload = new
            {
                UserId = userId,
                TotalKwhUsed = totalKwhUsed,
                TotalAmount = totalKwhUsed * ratePerKwh,
                RatePerKwh = ratePerKwh,
                ReadingCount = readingCount,
                LastUpdated = DateTime.UtcNow
            };

            var billingTopic = _topicOptions.GetBillingUpdateTopic(userId);

            if (!await EnsurePublisherConnectedAsync())
            {
                _logger.LogWarning("MQTT publisher unavailable; skipping billing broadcast for user {UserId}", userId);
                return;
            }

            await _mqttPublisher.PublishAsync(billingTopic, billingPayload);
            _logger.LogDebug("Published billing snapshot to MQTT topic: {Topic}", billingTopic);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish billing snapshot for user {UserId}", userId);
        }
    }

    private async Task<bool> EnsurePublisherConnectedAsync()
    {
        if (_mqttPublisher.IsConnected)
        {
            return true;
        }

        try
        {
            var connected = await _mqttPublisher.ConnectAsync();
            if (!connected)
            {
                _logger.LogWarning("MQTT publisher failed to connect");
            }
            return connected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting MQTT publisher");
            return false;
        }
    }
}
