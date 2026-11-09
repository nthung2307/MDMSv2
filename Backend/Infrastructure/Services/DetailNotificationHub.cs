using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services
{
    public class DetailNotificationHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
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

        [Authorize]
        public async Task SendUpdateToDetail(string itemId, object data)
        {
            await Clients.Group(itemId).SendAsync("ReceiveDetailUpdate", data);
        }
    }
}