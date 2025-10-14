using Microsoft.AspNetCore.SignalR;

namespace Server.Hubs;

public class BillingHub : Hub
{
    // TODO: Inject ConnectionRegistry service when billing services are implemented
    // private readonly ConnectionRegistry _connectionRegistry;

    public async Task Register(string userId)
    {
        // TODO: Store mapping using ConnectionRegistry service
        
        // For now, just acknowledge the registration
        await Clients.Caller.SendAsync("RegistrationConfirmed", userId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // TODO: Remove mapping using ConnectionRegistry service

        await base.OnDisconnectedAsync(exception);
    }
}