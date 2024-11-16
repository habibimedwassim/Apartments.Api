
namespace Apartments.Application.IServices;

public interface INotificationDispatcher
{
    Task SendBulkNotificationsAsync(List<string> userIds, string message, string type, string? status = null);
    Task SendNotificationAsync(string userId, string message, string type, string? status = null);
}