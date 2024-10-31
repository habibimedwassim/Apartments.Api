using Apartments.Application.Dtos.NotificationDtos;

namespace Apartments.Application.IServices;

public interface INotificationService
{
    Task SaveDeviceToken(string deviceToken);
    Task<IEnumerable<NotificationDto>> GetUnreadNotifications();
    Task MarkAsReadAsync(string type);
    Task SendNotificationToUser(NotifyUserRequest notifyUserRequest);
}
