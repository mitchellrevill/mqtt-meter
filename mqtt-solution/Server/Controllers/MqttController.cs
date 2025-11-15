using Microsoft.AspNetCore.Mvc;
using Infrastructure.Mqtt.Interfaces;
using Infrastructure.Mqtt.Configuration;
using Microsoft.Extensions.Options;

namespace Server.Controllers;

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
    /// Publish a meter reading to MQTT
    /// </summary>
    [HttpPost("publish/reading")]
    public async Task<IActionResult> PublishReading([FromBody] PublishReadingRequest request)
    {
        try
        {
            var reading = new
            {
                ClientId = request.UserId,
                Timestamp = DateTime.UtcNow,
                Value = request.Value,
                Unit = "kWh",
                Metadata = new Dictionary<string, object>
                {
                    { "source", "flutter" },
                    { "requestId", Guid.NewGuid().ToString() }
                }
            };

            // Publish to the readings topic
            var topic = $"meters/readings/{request.UserId}";
            await _publisher.PublishAsync(topic, reading);

            _logger.LogInformation("Published reading for user {UserId}: {Value} kWh to topic {Topic}", 
                request.UserId, request.Value, topic);

            return Ok(new
            {
                Success = true,
                Topic = topic,
                Message = "Reading published to MQTT successfully",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish reading for user {UserId}", request.UserId);
            return StatusCode(500, new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }
}

public class PublishReadingRequest
{
    public string UserId { get; set; } = string.Empty;
    public float Value { get; set; }
}
