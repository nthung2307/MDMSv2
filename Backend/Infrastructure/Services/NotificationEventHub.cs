using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services
{
    public class NotificationEventHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected to List Hub: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Client disconnected from List Hub: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendAllEvent(object data)
        {
            await Clients.All.SendAsync("ReceiveEvent", data);
        }
    }
}