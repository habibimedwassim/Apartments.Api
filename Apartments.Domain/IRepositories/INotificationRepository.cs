using Apartments.Domain.Entities;

namespace Apartments.Domain.IRepositories;

public interface INotificationRepository
{
    Task<Notification> AddNotificationAsync(Notification notification);
    Task AddNotificationListAsync(List<Notification> notifications);
    Task AddOrUpdateDeviceTokenAsync(string id, string deviceToken);
    Task<IEnumerable<Notification>> GetAllUnreadNotificationsAsync(string id);
    Task<List<string>> GetDeviceTokensByUserIdAsync(string userId);
    Task MarkAsReadAsync(string id, string type);
}
