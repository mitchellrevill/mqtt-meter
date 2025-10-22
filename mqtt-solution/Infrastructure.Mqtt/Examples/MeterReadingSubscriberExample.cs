using Infrastructure.Mqtt.Configuration;
using Infrastructure.Mqtt.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Mqtt.Examples;

/// <summary>
/// Example service showing how to subscribe to meter readings using shared subscriptions
/// </summary>
public class MeterReadingSubscriberExample : BackgroundService
{
    private readonly IMqttSubscriber _subscriber;
    private readonly ILogger<MeterReadingSubscriberExample> _logger;
    private readonly MqttTopicOptions _topicOptions;
    private readonly RabbitMqOptions _rabbitMqOptions;

    public MeterReadingSubscriberExample(
        IMqttSubscriber subscriber,
        ILogger<MeterReadingSubscriberExample> logger,
        IOptions<MqttTopicOptions> topicOptions,
        IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _subscriber = subscriber;
        _logger = logger;
        _topicOptions = topicOptions.Value;
        _rabbitMqOptions = rabbitMqOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Meter Reading Subscriber Example is starting");

        try
        {
            // Wait for subscriber to be ready
            await Task.Delay(2000, stoppingToken);

            // Example 1: Subscribe to all meter readings with shared subscription
            // This allows load balancing across multiple instances
            if (_rabbitMqOptions.EnableSharedSubscriptions)
            {
                await _subscriber.SubscribeSharedAsync<MeterReading>(
                    _rabbitMqOptions.SharedSubscriptionGroup,
                    $"{_topicOptions.ReadingsBaseTopic}/#",
                    HandleMeterReadingAsync,
                    stoppingToken);

                _logger.LogInformation("Subscribed to shared meter readings");
            }
            else
            {
                // Regular subscription (all instances receive all messages)
                await _subscriber.SubscribeAsync<MeterReading>(
                    _topicOptions.GetAllReadingsTopic(),
                    HandleMeterReadingAsync,
                    stoppingToken);

                _logger.LogInformation("Subscribed to all meter readings");
            }

            // Example 2: Subscribe to client status updates
            await _subscriber.SubscribeAsync<ClientStatus>(
                $"{_topicOptions.ClientStatusTopic}/#",
                HandleClientStatusAsync,
                stoppingToken);

            _logger.LogInformation("Subscribed to client status updates");

            // Example 3: Subscribe to alerts
            await _subscriber.SubscribeAsync<Alert>(
                $"{_topicOptions.AlertTopic}/#",
                HandleAlertAsync,
                stoppingToken);

            _logger.LogInformation("Subscribed to alerts");

            // Keep running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Meter Reading Subscriber Example is stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Meter Reading Subscriber Example");
        }
    }

    private async Task HandleMeterReadingAsync(string topic, MeterReading reading)
    {
        _logger.LogInformation(
            "Received meter reading from {Topic}: ClientId={ClientId}, Value={Value} {Unit}, Time={Timestamp}",
            topic, reading.ClientId, reading.Value, reading.Unit, reading.Timestamp);

        // Process the reading here
        // For example: save to database, calculate statistics, trigger alerts, etc.
        
        await Task.CompletedTask;
    }

    private async Task HandleClientStatusAsync(string topic, ClientStatus status)
    {
        _logger.LogInformation(
            "Received client status from {Topic}: ClientId={ClientId}, Status={Status}, Time={Timestamp}",
            topic, status.ClientId, status.Status, status.Timestamp);

        // Process the status update here
        // For example: update client state in database, trigger notifications, etc.
        
        await Task.CompletedTask;
    }

    private async Task HandleAlertAsync(string topic, Alert alert)
    {
        _logger.LogWarning(
            "Received alert from {Topic}: ClientId={ClientId}, Type={Type}, Severity={Severity}, Message={Message}",
            topic, alert.ClientId, alert.Type, alert.Severity, alert.Message);

        // Process the alert here
        // For example: send notifications, escalate critical alerts, log to monitoring system, etc.
        
        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Meter Reading Subscriber Example is stopping");
        await base.StopAsync(cancellationToken);
    }
}
