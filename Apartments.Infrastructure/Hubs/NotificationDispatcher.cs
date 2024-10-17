using Apartments.Application.IServices;
using Microsoft.AspNetCore.SignalR;

namespace Apartments.Infrastructure.Hubs;

public class NotificationHub : Hub
{

}
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
    public async Task SendBulkNotificationsAsync(List<string> userIds, string message, string type)
    {
        foreach (var userId in userIds)
        {
            await SendNotificationAsync(userId, message, type);
        }
    }
}
