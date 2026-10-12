using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services
{
    public class DetailNotificationDateTimeHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected to Detail Hub: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Client disconnected from Detail Hub: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SubscribeToDetail(string itemId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, itemId);
        }

        public async Task UnsubscribeFromDetail(string itemId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, itemId);
        }

        public async Task SendUpdateToDetail(string itemId, object data)
        {
            await Clients.Group(itemId).SendAsync("ReceiveDetailDateTimeUpdate", data);
        }
    }
}