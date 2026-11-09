using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services
{
    public class ListNotificationHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        [Authorize]
        public async Task SendUpdateToAll(object data)
        {
            await Clients.All.SendAsync("ReceiveUpdate", data);
        }
    }
}