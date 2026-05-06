using Microsoft.AspNetCore.SignalR;

namespace Notification.Api.Hubs;

public class NotificationHub : Hub
{
    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"[SignalR] Cliente conectado: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }
}
