
namespace Apartments.Application.IServices;

public interface IFcmService
{
    Task SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null);
    Task SendNotificationsToMultipleDevicesAsync(List<string> deviceTokens, string title, string body, Dictionary<string, string>? data = null);
}
