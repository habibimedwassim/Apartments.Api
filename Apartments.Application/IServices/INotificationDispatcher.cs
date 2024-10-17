namespace Apartments.Application.IServices;

public interface INotificationDispatcher
{
    Task SendNotificationAsync(string userId, string message, string type);
}