using Apartments.Domain.Entities;

namespace Apartments.Domain.IRepositories;

public interface INotificationRepository
{
    Task<Notification> AddNotificationAsync(Notification notification);
    Task AddNotificationListAsync(List<Notification> notifications);
    Task<IEnumerable<Notification>> GetAllUnreadNotificationsAsync(string id);
    Task MarkAsReadAsync(string id, string type);
}
