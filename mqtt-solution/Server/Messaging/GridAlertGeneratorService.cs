using Infrastructure.Mqtt.Interfaces;

namespace Server.Messaging;

/// <summary>
/// Background service that generates random grid alerts for demonstration purposes
/// </summary>
public class GridAlertGeneratorService : BackgroundService
{
    private readonly ILogger<GridAlertGeneratorService> _logger;
    private readonly IMqttPublisher _mqttPublisher;
    private readonly Random _random = new();

    private static readonly AlertTemplate[] AlertTemplates = new[]
    {
        new AlertTemplate("Scheduled maintenance starting in grid sector 4", "info", "Sector 4"),
        new AlertTemplate("High demand detected in your area", "warning", "Local Area"),
        new AlertTemplate("Grid overload detected - reduce consumption", "critical", "Multiple Sectors"),
        new AlertTemplate("Power quality monitoring active", "info", null),
        new AlertTemplate("Voltage fluctuation in distribution network", "warning", "Distribution Network"),
        new AlertTemplate("Emergency load shedding may occur", "critical", "All Areas"),
        new AlertTemplate("Grid capacity at 85%", "warning", null),
        new AlertTemplate("Renewable energy sources offline", "info", "Sector 7"),
        new AlertTemplate("Transformer temperature above normal", "warning", "Sector 2"),
        new AlertTemplate("Backup generators activated", "info", "Sector 5"),
        new AlertTemplate("Grid frequency instability detected", "critical", "Regional Grid"),
        new AlertTemplate("Scheduled grid capacity test in progress", "info", null),
    };

    public GridAlertGeneratorService(
        ILogger<GridAlertGeneratorService> logger,
        IMqttPublisher mqttPublisher)
    {
        _logger = logger;
        _mqttPublisher = mqttPublisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Grid Alert Generator Service is starting");

        // Wait for MQTT publisher to be ready
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        // Ensure publisher is connected
        if (!_mqttPublisher.IsConnected)
        {
            _logger.LogInformation("MQTT Publisher not connected, attempting to connect...");
            try
            {
                await _mqttPublisher.ConnectAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect MQTT publisher for alerts. Alert generation disabled.");
                return;
            }
        }

        _logger.LogInformation("Grid Alert Generator Service started successfully");

        // Generate first alert after 15-30 seconds
        var initialDelay = TimeSpan.FromSeconds(15 + _random.Next(16));
        _logger.LogInformation("First alert will be generated in {Seconds} seconds", initialDelay.TotalSeconds);
        await Task.Delay(initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await GenerateAndPublishAlert(stoppingToken);

                // Random interval between 30 and 45 seconds
                var nextInterval = TimeSpan.FromSeconds(30 + _random.Next(16));
                _logger.LogDebug("Next alert will be generated in {Seconds} seconds", nextInterval.TotalSeconds);
                await Task.Delay(nextInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when service is stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating grid alert");
                // Wait a bit before trying again
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("Grid Alert Generator Service is stopping");
    }

    private async Task GenerateAndPublishAlert(CancellationToken cancellationToken)
    {
        // Ensure publisher is still connected
        if (!_mqttPublisher.IsConnected)
        {
            _logger.LogWarning("MQTT Publisher is disconnected, attempting to reconnect...");
            try
            {
                await _mqttPublisher.ConnectAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconnect MQTT publisher");
                return;
            }
        }

        // Select a random alert template
        var template = AlertTemplates[_random.Next(AlertTemplates.Length)];

        var alert = new
        {
            alertId = $"alert-{Guid.NewGuid():N}",
            message = template.Message,
            severity = template.Severity,
            timestamp = DateTime.UtcNow.ToString("o"),
            affectedArea = template.AffectedArea
        };

        try
        {
            await _mqttPublisher.PublishAsync("alerts/grid", alert, cancellationToken: cancellationToken);
            
            _logger.LogInformation(
                "Generated and published grid alert: [{Severity}] {Message}",
                alert.severity.ToUpper(),
                alert.message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish grid alert");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Grid Alert Generator Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Template for grid alert generation
/// </summary>
internal record AlertTemplate(string Message, string Severity, string? AffectedArea);
