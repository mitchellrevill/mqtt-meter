using Infrastructure.Mqtt.Configuration;
using Infrastructure.Mqtt.Interfaces;

namespace Infrastructure.Mqtt.Examples;

/// <summary>
/// Example service showing how to publish meter readings
/// </summary>
public class MeterReadingPublisherExample
{
    private readonly IMqttPublisher _publisher;
    private readonly ILogger<MeterReadingPublisherExample> _logger;
    private readonly MqttTopicOptions _topicOptions;

    public MeterReadingPublisherExample(
        IMqttPublisher publisher,
        ILogger<MeterReadingPublisherExample> logger,
        IOptions<MqttTopicOptions> topicOptions)
    {
        _publisher = publisher;
        _logger = logger;
        _topicOptions = topicOptions.Value;
    }

    /// <summary>
    /// Publish a meter reading
    /// </summary>
    public async Task PublishMeterReadingAsync(string clientId, MeterReading reading, CancellationToken cancellationToken = default)
    {
        try
        {
            var topic = _topicOptions.GetMeterReadingTopic(clientId);
            await _publisher.PublishAsync(topic, reading, retain: false, cancellationToken);
            
            _logger.LogInformation("Published meter reading for client {ClientId}", clientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish meter reading for client {ClientId}", clientId);
            throw;
        }
    }

    /// <summary>
    /// Publish client status update
    /// </summary>
    public async Task PublishClientStatusAsync(string clientId, ClientStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            var topic = $"{_topicOptions.ClientStatusTopic}/{clientId}";
            await _publisher.PublishAsync(topic, status, retain: true, cancellationToken); // Retain status messages
            
            _logger.LogInformation("Published client status for {ClientId}: {Status}", clientId, status.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish client status for {ClientId}", clientId);
            throw;
        }
    }

    /// <summary>
    /// Publish alert
    /// </summary>
    public async Task PublishAlertAsync(string clientId, Alert alert, CancellationToken cancellationToken = default)
    {
        try
        {
            var topic = $"{_topicOptions.AlertTopic}/{clientId}";
            await _publisher.PublishAsync(topic, alert, retain: false, cancellationToken);
            
            _logger.LogInformation("Published alert for client {ClientId}: {AlertType}", clientId, alert.Type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish alert for client {ClientId}", clientId);
            throw;
        }
    }
}

// Example DTOs
public record MeterReading(
    string ClientId,
    DateTime Timestamp,
    double Value,
    string Unit,
    IDictionary<string, object>? Metadata = null);

public record ClientStatus(
    string ClientId,
    string Status,
    DateTime Timestamp,
    string? Message = null);

public record Alert(
    string ClientId,
    string Type,
    string Severity,
    string Message,
    DateTime Timestamp,
    IDictionary<string, object>? Data = null);
