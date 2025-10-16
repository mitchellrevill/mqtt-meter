using Microsoft.AspNetCore.Mvc;
using Server.Messaging;
using System.Text.Json;
using Domain.Entities;
using Application.Interfaces;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingsController : ControllerBase
{
    private readonly RabbitMqConnection _rabbitMq;
    private readonly ILogger<ReadingsController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IReadingService _readingService;

    public ReadingsController(
        RabbitMqConnection rabbitMq, 
        ILogger<ReadingsController> logger, 
        IHttpClientFactory httpClientFactory,
        IReadingService readingService)
    {
        _rabbitMq = rabbitMq;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _readingService = readingService;
    }

    [HttpPost]
    public async Task<IActionResult> PostReading([FromBody] CreateReadingDto request)
    {
        try
        {
            // Use Application layer to create Domain entity properly
            var reading = await _readingService.CreateAsync(request.UserId, (float)request.KwhSinceLast);

            var message = new ReadingMessage(
                request.UserId,
                reading.TimeStamp,
                request.KwhSinceLast
            );

            _rabbitMq.Publish(message);

            _logger.LogInformation("Reading published for user {UserId}: {KwhSinceLast} kWh", 
                request.UserId, request.KwhSinceLast);

            // Also add to billing system
            await AddToBilling(request.UserId, request.KwhSinceLast);

            return Ok(new { success = true, message = "Reading published successfully", readingId = reading.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing reading for user {UserId}", request.UserId);
            return StatusCode(500, new { success = false, message = "Failed to publish reading" });
        }
    }

    private async Task AddToBilling(string userId, double kwhUsed)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var usageRequest = new { UserId = userId, KwhUsed = kwhUsed };
            var json = JsonSerializer.Serialize(usageRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("http://localhost:5006/api/billing/add-usage", content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Added {KwhUsed} kWh to billing for user {UserId}", kwhUsed, userId);
            }
            else
            {
                _logger.LogWarning("Failed to add usage to billing for user {UserId}. Status: {StatusCode}", userId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding usage to billing for user {UserId}", userId);
        }
    }
}

// DTO for API communication - separate from Domain entities
public record CreateReadingDto(string UserId, double KwhSinceLast);

// DTO for returning reading information
public record ReadingDto(Guid Id, DateTime TimeStamp, float Value)
{
    public static ReadingDto FromDomain(Reading reading)
    {
        return new ReadingDto(reading.Id, reading.TimeStamp, reading.Value);
    }
}