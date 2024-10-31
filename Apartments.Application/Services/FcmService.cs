using Apartments.Application.IServices;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class FcmService(ILogger<FcmService> logger) : IFcmService
{
    public async Task SendNotificationAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null)
    {
        logger.LogInformation($"Sending notification to device: {deviceToken} with title: {title} and body: {body}");

        var message = new Message()
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data
        };

        await FirebaseMessaging.DefaultInstance.SendAsync(message);
    }
    public async Task SendNotificationsToMultipleDevicesAsync(List<string> deviceTokens, string title, string body, Dictionary<string, string>? data = null)
    {
        logger.LogInformation($"Sending notification to multiple devices with title: {title} and body: {body}");
        var message = new MulticastMessage()
        {
            Tokens = deviceTokens,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data
        };

        await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
    }
}
