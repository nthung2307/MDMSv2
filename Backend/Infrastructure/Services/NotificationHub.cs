using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services
{
    public class NotificationHub : Hub
    {
        // Handle connection events
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        // Send updates to all clients (List view scenario)
        public async Task SendListUpdate(object data)
        {
            await Clients.All.SendAsync("ReceiveListUpdate", data);
        }

        // Subscribe to a group for detail view
        public async Task SubscribeToDetail(string itemId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, itemId);
        }

        // Unsubscribe from a group for detail view
        public async Task UnsubscribeFromDetail(string itemId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, itemId);
        }

        // Send updates to specific group (Detail view scenario)
        public async Task SendDetailUpdate(string itemId, object data)
        {
            await Clients.Group(itemId).SendAsync("ReceiveDetailUpdate", data);
        }
    }
}