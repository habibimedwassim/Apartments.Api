using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Application.Dtos.NotificationDtos;
using Apartments.Application.IServices;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Apartments.Application.Services;

public class NotificationService(
    ILogger<NotificationService> logger,
    IMapper mapper,
    IUserContext userContext,
    IFcmService fcmService,
    INotificationRepository notificationRepository
    ) : INotificationService
{
    public async Task<PagedResult<NotificationDto>> GetAllNotifications(int pageNumber)
    {
        logger.LogInformation("Retrieving All Notifications");
        var currentUser = userContext.GetCurrentUser();

        if (!currentUser.IsUser) return new PagedResult<NotificationDto>([], 0, pageNumber); ;

        PagedModel<Notification> pagedModel = await notificationRepository.GetNotificationsPagedAsync(pageNumber, currentUser.Id);

        var notificationDtos = mapper.Map<IEnumerable<NotificationDto>>(pagedModel.Data);

        var result =
            new PagedResult<NotificationDto>(notificationDtos, pagedModel.DataCount, pageNumber);

        return result;
    }
    public async Task SaveDeviceToken(string deviceToken)
    {
        var currentUser = userContext.GetCurrentUser();

        if (!currentUser.IsUser) return;

        await notificationRepository.AddOrUpdateDeviceTokenAsync(currentUser.Id, deviceToken);
    }

    public async Task<IEnumerable<NotificationDto>> GetUnreadNotifications()
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Retrieving User ({Email}) unread notifications", currentUser.Email);

        var notifications = await notificationRepository.GetAllUnreadNotificationsAsync(currentUser.Id);

        var notificationsDtos = mapper.Map<IEnumerable<NotificationDto>>(notifications);

        return notificationsDtos;
    }

    public async Task MarkAsReadAsync(string type)
    {
        var currentUser = userContext.GetCurrentUser();
        var requestType = CoreUtilities.ValidateEnumToString<NotificationType>(type);

        logger.LogInformation("Marking notifications as read for User ({Email})", currentUser.Email);

        await notificationRepository.MarkAsReadAsync(currentUser.Id, requestType.ToLower());
    }
    public async Task MarkAsReadAsync(int id)
    {
        logger.LogInformation("Marking notification {Id} as read", id);

        await notificationRepository.MarkAsReadAsync(id);
    }
    public async Task SendNotificationToUser(NotifyUserRequest notifyUserRequest)
    {
        var deviceTokens = await notificationRepository.GetDeviceTokensByUserIdAsync(notifyUserRequest.UserId);

        if (deviceTokens.Count > 0)
        {
            await fcmService.SendNotificationsToMultipleDevicesAsync(deviceTokens, notifyUserRequest.Title, notifyUserRequest.Body);
        }
    }

    public async Task<ServiceResult<UnreadCount>> GetUnreadNotificationsCount()
    {
        var currentUser = userContext.GetCurrentUser();
        if(currentUser == null)
        {
            return ServiceResult<UnreadCount>.SuccessResult(new() { Count = 0});
        }

        int count = await notificationRepository.GetUnreadNotificationsCountAsync(currentUser.Id);
        return ServiceResult<UnreadCount>.SuccessResult(new() { Count = count });
    }
}
