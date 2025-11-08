using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Application.Interfaces;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingsController : ControllerBase
{
    private readonly ILogger<ReadingsController> _logger;
    private readonly IReadingService _readingService;

    public ReadingsController(
        ILogger<ReadingsController> logger,
        IReadingService readingService)
    {
        _logger = logger;
        _readingService = readingService;
    }

    /// <summary>
    /// Get all readings
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var readings = await _readingService.GetAll();
            return Ok(readings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all readings");
            return StatusCode(500, new { Error = "Failed to retrieve readings" });
        }
    }

    /// <summary>
    /// Create a new reading
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReading([FromBody] CreateReadingRequest request)
    {
        try
        {
            // Persist the reading via the application service (which routes to MediatR/Repository)
            var savedReading = await _readingService.CreateAsync(request.UserId, request.Value);

            _logger.LogInformation("Persisted reading to DB: {Value} kWh for user {UserId}", request.Value, request.UserId);

            return Ok(new
            {
                Success = true,
                Message = "Reading created and persisted",
                Reading = savedReading
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reading");
            return StatusCode(500, new { Error = "Failed to create reading" });
        }
    }
}

public class CreateReadingRequest
{
    public string UserId { get; set; } = string.Empty;
    public float Value { get; set; }
}
