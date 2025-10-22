using Infrastructure.Mqtt.Interfaces;
using Infrastructure.Mqtt.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DemoWeb.Server.Controllers;

/// <summary>
/// Controller for MQTT operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MqttController : ControllerBase
{
    private readonly IMqttPublisher _publisher;
    private readonly ILogger<MqttController> _logger;
    private readonly MqttTopicOptions _topicOptions;

    public MqttController(
        IMqttPublisher publisher,
        ILogger<MqttController> logger,
        IOptions<MqttTopicOptions> topicOptions)
    {
        _publisher = publisher;
        _logger = logger;
        _topicOptions = topicOptions.Value;
    }

    /// <summary>
    /// Get MQTT connection status
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            Connected = _publisher.IsConnected,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Publish a test meter reading
    /// </summary>
    [HttpPost("publish/reading")]
    public async Task<IActionResult> PublishReading([FromBody] PublishReadingRequest request)
    {
        try
        {
            var reading = new
            {
                request.ClientId,
                Timestamp = DateTime.UtcNow,
                request.Value,
                request.Unit,
                Metadata = new Dictionary<string, object>
                {
                    { "source", "api" },
                    { "requestId", Guid.NewGuid().ToString() }
                }
            };

            var topic = _topicOptions.GetMeterReadingTopic(request.ClientId);
            await _publisher.PublishAsync(topic, reading);

            _logger.LogInformation("Published reading for client {ClientId}: {Value} {Unit}", 
                request.ClientId, request.Value, request.Unit);

            return Ok(new
            {
                Success = true,
                Topic = topic,
                Message = "Reading published successfully",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish reading for client {ClientId}", request.ClientId);
            return StatusCode(500, new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Publish a client status update
    /// </summary>
    [HttpPost("publish/status")]
    public async Task<IActionResult> PublishStatus([FromBody] PublishStatusRequest request)
    {
        try
        {
            var status = new
            {
                request.ClientId,
                request.Status,
                Timestamp = DateTime.UtcNow,
                request.Message
            };

            var topic = $"{_topicOptions.ClientStatusTopic}/{request.ClientId}";
            await _publisher.PublishAsync(topic, status, retain: true);

            _logger.LogInformation("Published status for client {ClientId}: {Status}", 
                request.ClientId, request.Status);

            return Ok(new
            {
                Success = true,
                Topic = topic,
                Message = "Status published successfully",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish status for client {ClientId}", request.ClientId);
            return StatusCode(500, new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Publish an alert
    /// </summary>
    [HttpPost("publish/alert")]
    public async Task<IActionResult> PublishAlert([FromBody] PublishAlertRequest request)
    {
        try
        {
            var alert = new
            {
                request.ClientId,
                request.Type,
                request.Severity,
                request.Message,
                Timestamp = DateTime.UtcNow,
                Data = request.Data ?? new Dictionary<string, object>()
            };

            var topic = $"{_topicOptions.AlertTopic}/{request.ClientId}";
            await _publisher.PublishAsync(topic, alert);

            _logger.LogWarning("Published alert for client {ClientId}: {Type} - {Severity}", 
                request.ClientId, request.Type, request.Severity);

            return Ok(new
            {
                Success = true,
                Topic = topic,
                Message = "Alert published successfully",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish alert for client {ClientId}", request.ClientId);
            return StatusCode(500, new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Publish multiple readings in batch
    /// </summary>
    [HttpPost("publish/readings/batch")]
    public async Task<IActionResult> PublishReadingsBatch([FromBody] PublishReadingsBatchRequest request)
    {
        try
        {
            var publishTasks = request.Readings.Select(async reading =>
            {
                var readingData = new
                {
                    reading.ClientId,
                    Timestamp = DateTime.UtcNow,
                    reading.Value,
                    reading.Unit
                };

                var topic = _topicOptions.GetMeterReadingTopic(reading.ClientId);
                await _publisher.PublishAsync(topic, readingData);
            });

            await Task.WhenAll(publishTasks);

            _logger.LogInformation("Published {Count} readings in batch", request.Readings.Count);

            return Ok(new
            {
                Success = true,
                Count = request.Readings.Count,
                Message = "Readings published successfully",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish readings batch");
            return StatusCode(500, new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }
}

// Request DTOs
public record PublishReadingRequest(
    string ClientId,
    double Value,
    string Unit = "kWh");

public record PublishStatusRequest(
    string ClientId,
    string Status,
    string? Message = null);

public record PublishAlertRequest(
    string ClientId,
    string Type,
    string Severity,
    string Message,
    Dictionary<string, object>? Data = null);

public record PublishReadingsBatchRequest(
    List<PublishReadingRequest> Readings);
