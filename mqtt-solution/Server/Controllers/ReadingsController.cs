using Microsoft.AspNetCore.Mvc;
using Server.Messaging;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingsController : ControllerBase
{
    private readonly RabbitMqConnection _rabbitMq;
    private readonly ILogger<ReadingsController> _logger;

    public ReadingsController(RabbitMqConnection rabbitMq, ILogger<ReadingsController> logger)
    {
        _rabbitMq = rabbitMq;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PostReading([FromBody] ReadingRequest request)
    {
        try
        {
            var message = new ReadingMessage(
                request.UserId,
                DateTime.UtcNow,
                request.KwhSinceLast
            );

            _rabbitMq.Publish(message);

            _logger.LogInformation("Reading published for user {UserId}: {KwhSinceLast} kWh", 
                request.UserId, request.KwhSinceLast);

            return Ok(new { success = true, message = "Reading published successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing reading for user {UserId}", request.UserId);
            return StatusCode(500, new { success = false, message = "Failed to publish reading" });
        }
    }
}

public record ReadingRequest(string UserId, double KwhSinceLast);