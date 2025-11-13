using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs;

public class BillingHub : Hub
{
    private readonly ILogger<BillingHub> _logger;

    public BillingHub(ILogger<BillingHub> logger)
    {
        _logger = logger;
    }

    public async Task Register(string userId)
    {
        // Add the connection to a group based on userId
        await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        
        _logger.LogInformation("User {UserId} registered with connection {ConnectionId}", userId, Context.ConnectionId);
        
        // Acknowledge the registration
        await Clients.Caller.SendAsync("RegistrationConfirmed", userId);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}