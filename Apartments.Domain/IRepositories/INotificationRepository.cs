using Apartments.Domain.Entities;

namespace Apartments.Domain.IRepositories;

public interface INotificationRepository
{
    Task<Notification> AddNotificationAsync(Notification notification);
    Task<IEnumerable<Notification>> GetNotificationsAsync(string id);
    Task MarkAsReadAsync(string id, string type);
}
