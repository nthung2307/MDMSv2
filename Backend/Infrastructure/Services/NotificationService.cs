using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Services;

public class NotificationService : BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = $"Server Notification: {DateTime.Now}";
            // Console.WriteLine(message); // Log to confirm the service is running

            // await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);

            await Task.Delay(5000);
        }
    }
}