using Apartments.Domain.Common;
using Apartments.Domain.Entities;

namespace Apartments.Domain.IRepositories;

public interface INotificationRepository
{
    Task<Notification> AddNotificationAsync(Notification notification);
    Task AddNotificationListAsync(List<Notification> notifications);
    Task AddOrUpdateDeviceTokenAsync(string id, string deviceToken);
    Task<IEnumerable<Notification>> GetAllUnreadNotificationsAsync(string id);
    Task<List<string>> GetDeviceTokensByUserIdAsync(string userId);
    Task<PagedModel<Notification>> GetNotificationsPagedAsync(int pageNumber, string id);
    Task<int> GetUnreadNotificationsCountAsync(string id);
    Task MarkAsReadAsync(string id, string type);
    Task MarkAsReadAsync(int id);
}
