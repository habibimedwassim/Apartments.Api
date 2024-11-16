using Apartments.Application.Common;
using Apartments.Application.Dtos.NotificationDtos;
using Apartments.Domain.Common;

namespace Apartments.Application.IServices;

public interface INotificationService
{
    Task SaveDeviceToken(string deviceToken);
    Task<IEnumerable<NotificationDto>> GetUnreadNotifications();
    Task MarkAsReadAsync(string type);
    Task MarkAsReadAsync(int id);
    Task SendNotificationToUser(NotifyUserRequest notifyUserRequest);
    Task<PagedResult<NotificationDto>> GetAllNotifications(int pageNumber);
    Task<ServiceResult<UnreadCount>> GetUnreadNotificationsCount();
}
