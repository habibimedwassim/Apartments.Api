using Apartments.Application.Common;
using Apartments.Application.Dtos.NotificationDtos;
using Apartments.Application.IServices;
using Apartments.Domain.IRepositories;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class NotificationService(
    ILogger<NotificationService> logger,
    IMapper mapper,
    IUserContext userContext,
    INotificationRepository notificationRepository
    ) : INotificationService
{
    public async Task<IEnumerable<NotificationDto>> GetUnreadNotifications()
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Retrieving User ({Email}) unread notifications", currentUser.Email);

        var notifications = await notificationRepository.GetNotificationsAsync(currentUser.Id);

        var notificationsDtos = mapper.Map<IEnumerable<NotificationDto>>(notifications);

        return notificationsDtos;
    }

    public async Task MarkAsReadAsync(string type)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Marking notifications as read for User ({Email})", currentUser.Email);

        await notificationRepository.MarkAsReadAsync(currentUser.Id, type);
    }
}
