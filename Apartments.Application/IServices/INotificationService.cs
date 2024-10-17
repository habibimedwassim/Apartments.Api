using Apartments.Application.Dtos.NotificationDtos;

namespace Apartments.Application.IServices;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetUnreadNotifications();
    Task MarkAsReadAsync(string type);
}
