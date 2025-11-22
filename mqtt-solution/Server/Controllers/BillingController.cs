using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly ILogger<BillingController> _logger;
    private readonly IReadingService _readingService;

    public BillingController(
        ILogger<BillingController> logger,
        IReadingService readingService)
    {
        _logger = logger;
        _readingService = readingService;
    }

    /// <summary>
    /// Get billing information for a user
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetBilling(string userId)
    {
        try
        {
            // Get all readings
            var readings = await _readingService.GetAll();
            
            var userReadings = readings
            .Where(r => r.UserId == userId)
            .ToList();
            var userReadings = readings.ToList();

            // Calculate billing
            const double ratePerKwh = 0.15;
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
            // In a real implementation, this would delete or archive readings
            // For now, we'll just return success
            _logger.LogInformation("Billing reset requested for user {UserId}", userId);

            return Ok(new
            {
                Success = true,
                Message = $"Billing reset for user {userId}",
                Timestamp = DateTime.UtcNow
            });
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
