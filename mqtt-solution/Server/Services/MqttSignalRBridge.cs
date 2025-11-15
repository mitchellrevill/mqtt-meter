using Microsoft.AspNetCore.SignalR;
using Server.Hubs;

namespace Server.Services;

/// <summary>
/// Service to bridge MQTT messages to SignalR clients
/// </summary>
public interface IMqttSignalRBridge
{
    Task BroadcastMeterReading(string userId, object reading);
    Task BroadcastBillingUpdate(string userId, object billing);
}

public class MqttSignalRBridge : IMqttSignalRBridge
{
    private readonly IHubContext<BillingHub> _hubContext;
    private readonly ILogger<MqttSignalRBridge> _logger;

    public MqttSignalRBridge(
        IHubContext<BillingHub> hubContext,
        ILogger<MqttSignalRBridge> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastMeterReading(string userId, object reading)
    {
        try
        {
            await _hubContext.Clients.Group(userId).SendAsync("MeterReading", reading);
            _logger.LogDebug("Broadcasted meter reading to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast meter reading to user {UserId}", userId);
        }
    }

    public async Task BroadcastBillingUpdate(string userId, object billing)
    {
        try
        {
            await _hubContext.Clients.Group(userId).SendAsync("BillingUpdate", billing);
            _logger.LogDebug("Broadcasted billing update to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast billing update to user {UserId}", userId);
        }
    }
}
