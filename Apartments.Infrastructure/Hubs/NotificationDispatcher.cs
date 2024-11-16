using Apartments.Application.IServices;
using Microsoft.AspNetCore.SignalR;

namespace Apartments.Infrastructure.Hubs;

public class NotificationHub : Hub
{

}
public class NotificationDispatcher(IHubContext<NotificationHub> hubContext) : INotificationDispatcher
{

    public async Task SendNotificationAsync(string userId, string message, string type, string? status = null)
    {
        var notification = new
        {
            Message = message,
            Type = type,
            Status = status
        };

        await hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
    }
    public async Task SendBulkNotificationsAsync(List<string> userIds, string message, string type, string? status = null)
    {
        foreach (var userId in userIds)
        {
            await SendNotificationAsync(userId, message, type, status);
        }
    }
}
