using System.Text;
using System.Text.Json;
using Infrastructure.Mqtt.Configuration;
using Infrastructure.Mqtt.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MqttTestClient.Options;

namespace MqttTestClient.Workers;

public class TestClientWorker : BackgroundService
{
    private readonly ILogger<TestClientWorker> _logger;
    private readonly IMqttPublisher _publisher;
    private readonly IMqttSubscriber _subscriber;
    private readonly MqttTopicOptions _topics;
    private readonly TestClientOptions _options;
    private readonly Random _random = new();

    public TestClientWorker(
        ILogger<TestClientWorker> logger,
        IMqttPublisher publisher,
        IMqttSubscriber subscriber,
        IOptions<MqttTopicOptions> topicOptions,
        IOptions<TestClientOptions> options)
    {
        _logger = logger;
        _publisher = publisher;
        _subscriber = subscriber;
        _topics = topicOptions.Value;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting MQTT test client for {ClientId}", _options.ClientId);

        await _subscriber.StartAsync(stoppingToken);
        await SubscribeToBillingUpdatesAsync(stoppingToken);

        if (_options.PublishStatusMessages)
        {
            await PublishStatusAsync("online", "Test client connected", stoppingToken);
        }

        var delay = TimeSpan.FromSeconds(Math.Max(1, _options.PublishIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishReadingAsync(stoppingToken);
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish reading");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_options.PublishStatusMessages)
        {
            await PublishStatusAsync("offline", "Test client disconnected", cancellationToken);
        }

        await _subscriber.StopAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    private async Task PublishReadingAsync(CancellationToken cancellationToken)
    {
        var range = Math.Max(_options.MaximumValue - _options.MinimumValue, 0.1);
        var value = Math.Round(_options.MinimumValue + (_random.NextDouble() * range), 3);

        var payload = new MeterReadingPayload
        {
            ClientId = _options.ClientId,
            Timestamp = DateTime.UtcNow,
            Value = value,
            Unit = _options.Unit,
            Metadata = new Dictionary<string, object>
            {
                ["source"] = "mqtt-test-client",
                ["messageId"] = Guid.NewGuid().ToString("N")
            }
        };

        var topic = _topics.GetMeterReadingTopic(_options.ClientId);
        await _publisher.PublishAsync(topic, payload, cancellationToken: cancellationToken);
        _logger.LogInformation("Published reading {Value} {Unit} to {Topic}", value, _options.Unit, topic);
    }

    private async Task SubscribeToBillingUpdatesAsync(CancellationToken cancellationToken)
    {
        var topic = _topics.GetBillingUpdateTopic(_options.ClientId);
        await _subscriber.SubscribeAsync(topic, async (receivedTopic, payload) =>
        {
            var json = Encoding.UTF8.GetString(payload);
            try
            {
                using var doc = JsonDocument.Parse(json);
                var total = doc.RootElement.GetProperty("TotalAmount").GetDouble();
                var kwh = doc.RootElement.GetProperty("TotalKwhUsed").GetDouble();
                _logger.LogInformation("Billing update received | kWh: {Kwh:F2}, total: ${Total:F2}", kwh, total);
            }
            catch
            {
                _logger.LogInformation("Billing update received on {Topic}: {Payload}", receivedTopic, json);
            }
        }, cancellationToken);

        _logger.LogInformation("Subscribed to billing topic {Topic}", topic);
    }

    private async Task PublishStatusAsync(string status, string message, CancellationToken cancellationToken)
    {
        var topic = $"{_topics.ClientStatusTopic}/{_options.ClientId}";
        var payload = new ClientStatusPayload
        {
            ClientId = _options.ClientId,
            Status = status,
            Timestamp = DateTime.UtcNow,
            Message = message
        };

        await _publisher.PublishAsync(topic, payload, retain: true, cancellationToken: cancellationToken);
        _logger.LogInformation("Published status {Status} to {Topic}", status, topic);
    }

    private sealed class MeterReadingPayload
    {
        public string ClientId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; } = "kWh";
        public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    private sealed class ClientStatusPayload
    {
        public string ClientId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
