using Apartments.Application.Dtos.NotificationDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Utilities;

public interface INotificationUtilities
{
    Task SendNotificationAsync(NotificationModel notificationModel);
}

public class NotificationUtilities(
    ILogger<NotificationUtilities> logger,
    IEmailService emailService,
    INotificationRepository notificationRepository,
    INotificationService notificationService,
    INotificationDispatcher notificationDispatcher
    ) : INotificationUtilities
{
    public async Task SendNotificationAsync(NotificationModel notificationModel)
    {
        try
        {
            if (notificationModel.StoreDb)
            {
                var notification = new Notification
                {
                    UserId = notificationModel.UserId,
                    Message = notificationModel.Message,
                    Type = notificationModel.NotificationType,
                    IsRead = false
                };
                await notificationRepository.AddNotificationAsync(notification);
            }

            if (notificationModel.SendSignalR)
            {
                await notificationDispatcher.SendNotificationAsync(
                    notificationModel.UserId, 
                    notificationModel.Message, 
                    notificationModel.NotificationType, 
                    notificationModel.Status);
            }

            if (notificationModel.SendFirebase)
            {
                await notificationService.SendNotificationToUser(new NotifyUserRequest
                {
                    UserId = notificationModel.UserId,
                    Title = notificationModel.Title,
                    Body = notificationModel.Message
                });
            }

            if (notificationModel.SendEmail)
            {
                await emailService.SendEmailAsync(notificationModel.Email, notificationModel.Title, notificationModel.Message);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Failed to send notification to user {notificationModel.Email}: {ex.Message}");
        }
    }
}
