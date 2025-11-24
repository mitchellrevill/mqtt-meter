using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Infrastructure.Mqtt.Interfaces;
using Infrastructure.Mqtt.Configuration;
using Microsoft.Extensions.Options;
using Infrastructure.Mqtt.DatabaseContext;
using Infrastructure.Mqtt.Services;
using Infrastructure.Mqtt.Services.Mocking;
using Domain.Entities;
using Bogus;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly ILogger<BillingController> _logger;
    private readonly MqttDbContext _Dbcontext;
    private readonly IReadingService _readingService;
    private readonly IMqttPublisher _mqttPublisher;
    private readonly MqttTopicOptions _topicOptions;

    const double ratePerKwh = 0.15;

    public BillingController(
        ILogger<BillingController> logger,
        IReadingService readingService,
        IMqttPublisher mqttPublisher,
        MqttDbContext Dbcontext,
        IOptions<MqttTopicOptions> topicOptions)
    {
        _logger = logger;
        _readingService = readingService;
        _mqttPublisher = mqttPublisher;
        _Dbcontext = Dbcontext;
        _topicOptions = topicOptions.Value;
    }

    /// <summary>
    /// Generate seed billings for a user
    /// </summary>
    [HttpGet("{userId}/Seed")]
    public async Task<IActionResult> SeedBilling(string userId)
    {
        try
        {
            Random random = new Random();
            int randomNumber = random.Next(0, 20);
            double cumulativeTotal = 0;
            List<object> billingList = new();

            _Dbcontext.Database.EnsureCreated();


            for(int i = 0; i < randomNumber; i++)
            {
                var fakeReadings = new ReadingGenerator(userId).GenerateBetween(2, 20);
                GeneratorHelper.AddEntities<Reading>(fakeReadings, _Dbcontext);

                // Calculate billing
                var totalKwhUsed = (double)fakeReadings.Sum(r => r.Value);
                var totalAmount = totalKwhUsed * ratePerKwh;
                var readingCount = fakeReadings.Count;

                var billing = new
                {
                    UserId = userId,
                    TotalKwhUsed = totalKwhUsed,
                    TotalAmount = totalAmount,
                    RatePerKwh = ratePerKwh,
                    ReadingCount = readingCount,
                    LastUpdated = DateTime.UtcNow
                };

                billingList.Add(billing);

                cumulativeTotal += totalAmount;

                // Publish billing update to MQTT
                try
                {
                    var billingTopic = _topicOptions.GetBillingUpdateTopic(userId);
                    await _mqttPublisher.PublishAsync(billingTopic, billing);
                    _logger.LogDebug("Published billing update to MQTT topic: {Topic}", billingTopic);
                }
                catch (Exception mqttEx)
                {
                    _logger.LogWarning(mqttEx, "Failed to publish billing update to MQTT for user {UserId}", userId);
                }
            }

            _logger.LogInformation("Seed billings added for {UserId}: {Amount:C}", userId, cumulativeTotal);

            return Ok(billingList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing for user {UserId}", userId);
            return StatusCode(500, new { Error = "Failed to retrieve billing information" });
        }
    }

    /// <summary>
    /// Get billing information for a user
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetBilling(string userId)
    {
        try
        {
            await SeedBilling(userId);
            // Get all readings
            var readings = await _readingService.GetAll();
            
            var userReadings = readings
                .Where(r => r.UserId == userId)
                .ToList();

            // Calculate billing
            var totalKwhUsed = (double)userReadings.Sum(r => r.Value);
            var totalAmount = totalKwhUsed * ratePerKwh;
            var readingCount = userReadings.Count;

            var billing = new
            {
                UserId = userId,
                TotalKwhUsed = totalKwhUsed,
                TotalAmount = totalAmount,
                RatePerKwh = ratePerKwh,
                ReadingCount = readingCount,
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation("Retrieved billing for user {UserId}: {Amount:C}", userId, totalAmount);

            // Publish billing update to MQTT
            try
            {
                var billingTopic = _topicOptions.GetBillingUpdateTopic(userId);
                await _mqttPublisher.PublishAsync(billingTopic, billing);
                _logger.LogDebug("Published billing update to MQTT topic: {Topic}", billingTopic);
            }
            catch (Exception mqttEx)
            {
                _logger.LogWarning(mqttEx, "Failed to publish billing update to MQTT for user {UserId}", userId);
            }

            return Ok(billing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing for user {UserId}", userId);
            return StatusCode(500, new { Error = "Failed to retrieve billing information" });
        }
    }

    /// <summary>
    /// Reset billing for a user
    /// </summary>
    [HttpPost("{userId}/reset")]
    public async Task<IActionResult> ResetBilling(string userId)
    {
        try
        {
            _logger.LogInformation("Billing reset requested for user {UserId}", userId);

            var response = new
            {
                Success = true,
                Message = $"Billing reset for user {userId}",
                Timestamp = DateTime.UtcNow
            };

            // Perform reset in the application data store
            try
            {
                await _readingService.ResetForUserAsync(userId);
                _logger.LogInformation("Cleared readings for user {UserId} as part of reset", userId);
            }
            catch (Exception resetEx)
            {
                _logger.LogWarning(resetEx, "Failed to reset stored readings for user {UserId}", userId);
            }

            // Publish reset notification to MQTT
            try
            {
                var billingTopic = _topicOptions.GetBillingUpdateTopic(userId);
                await _mqttPublisher.PublishAsync(billingTopic, new
                {
                    UserId = userId,
                    Type = "Reset",
                    TotalKwhUsed = 0.0,
                    TotalAmount = 0.0,
                    RatePerKwh = ratePerKwh,
                    ReadingCount = 0,
                    LastUpdated = DateTime.UtcNow
                });
                _logger.LogDebug("Published billing reset to MQTT topic: {Topic}", billingTopic);
            }
            catch (Exception mqttEx)
            {
                _logger.LogWarning(mqttEx, "Failed to publish billing reset to MQTT for user {UserId}", userId);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting billing for user {UserId}", userId);
            return StatusCode(500, new
            {
                Success = false,
                Message = "Failed to reset billing",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get billing history (paginated)
    /// </summary>
    [HttpGet("{userId}/history")]
    public async Task<IActionResult> GetBillingHistory(
        string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var readings = await _readingService.GetAll();
            
            // TODO: Filter by userId once Reading entity has ClientId/UserId
            var userReadings = readings
                .OrderByDescending(r => r.TimeStamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Value,
                    Timestamp = r.TimeStamp,
                    Cost = r.Value * 0.15
                })
                .ToList();

            return Ok(new
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize,
                Readings = userReadings
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting billing history for user {UserId}", userId);
            return StatusCode(500, new { Error = "Failed to retrieve billing history" });
        }
    }
}
