using Apartments.Application.IServices;
using Microsoft.AspNetCore.SignalR;

namespace Apartments.Infrastructure.Hubs;

public class NotificationDispatcher(IHubContext<NotificationHub> hubContext) : INotificationDispatcher
{

    public async Task SendNotificationAsync(string userId, string message, string type)
    {
        await hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
        {
            Message = message,
            Type = type
        });
    }
}
