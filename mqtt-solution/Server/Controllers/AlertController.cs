using Microsoft.AspNetCore.Mvc;
using Infrastructure.Mqtt.Interfaces;
using Infrastructure.Mqtt.Configuration;
using Microsoft.Extensions.Options;

namespace Server.Controllers;

/// <summary>
/// Controller for managing grid alerts
/// TODO: This is a placeholder for future implementation of grid alert monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AlertController : ControllerBase
{
    private readonly ILogger<AlertController> _logger;
    private readonly IMqttPublisher _mqttPublisher;
    private readonly MqttTopicOptions _topicOptions;

    public AlertController(
        ILogger<AlertController> logger,
        IMqttPublisher mqttPublisher,
        IOptions<MqttTopicOptions> topicOptions)
    {
        _logger = logger;
        _mqttPublisher = mqttPublisher;
        _topicOptions = topicOptions.Value;
    }

    /// <summary>
    /// Publish a grid alert (mock endpoint for testing)
    /// </summary>

    /// Sample request:
    /// POST /api/alert/publish
    /// {
    ///   "message": "High demand detected in your area",
    ///   "severity": "warning",
    ///   "affectedArea": "Local Area"
    /// }
    /// </remarks>
    [HttpPost("publish")]
    public async Task<IActionResult> PublishAlert([FromBody] PublishAlertRequest request)
    {
        if (string.IsNullOrEmpty(request.Message))
        {
            return BadRequest(new { error = "Message is required" });
        }

        var alertPayload = new
        {
            alertId = $"alert-{Guid.NewGuid():N}",
            message = request.Message,
            severity = request.Severity ?? "info",
            timestamp = DateTime.UtcNow.ToString("o"),
            affectedArea = request.AffectedArea
        };

        try
        {
            await _mqttPublisher.PublishAsync("alerts/grid", alertPayload);
            
            _logger.LogInformation(
                "Published grid alert: {Message} (Severity: {Severity})",
                request.Message,
                request.Severity
            );

            return Ok(new
            {
                success = true,
                alertId = alertPayload.alertId,
                message = "Alert published successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish alert");
            return StatusCode(500, new { error = "Failed to publish alert" });
        }
    }

    /// <summary>
    /// Get alert configuration status
    /// TODO: Implement real grid monitoring integration
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetAlertStatus()
    {
        return Ok(new
        {
            enabled = false,
            message = "Alert monitoring not yet implemented.",
            mqttConnected = _mqttPublisher.IsConnected,
            timestamp = DateTime.UtcNow
        });
    }
}

/// <summary>
/// Request model for publishing alerts
/// </summary>
public class PublishAlertRequest
{
    /// <summary>
    /// Alert message to display
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Alert severity level: "info", "warning", or "critical"
    /// </summary>
    public string? Severity { get; set; }

    /// <summary>
    /// Optional: Area affected by the alert
    /// </summary>
    public string? AffectedArea { get; set; }
}
