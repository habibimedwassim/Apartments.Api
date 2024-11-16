using Apartments.Application.Common;
using Apartments.Application.Dtos.NotificationDtos;
using Apartments.Application.IServices;
using Apartments.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartments.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]

public class NotificationController(INotificationService notificationService) : ControllerBase
{
    [HttpGet("all")]
    public async Task<IActionResult> GetAllNotifications([FromQuery] int pageNumber)
    {
        var result = await notificationService.GetAllNotifications(pageNumber);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        var result = await notificationService.GetUnreadNotifications();
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadNotificationsCount()
    {
        ServiceResult<UnreadCount> result = await notificationService.GetUnreadNotificationsCount();
        return Ok(result.Data);
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> MarkAsRead([FromRoute] int id)
    {
        await notificationService.MarkAsReadAsync(id);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> MarkAllAsReadByType([FromQuery] string type)
    {
        await notificationService.MarkAsReadAsync(type);
        return Ok();
    }

    [HttpPost("save-device")]
    public async Task<IActionResult> SaveDeviceToken([FromBody] SaveDeviceTokenRequest request)
    {
        await notificationService.SaveDeviceToken(request.DeviceToken);
        return Ok();
    }

    [HttpPost("send")]
    public async Task<IActionResult> NotifyUser([FromBody] NotifyUserRequest notifyUserRequest)
    {
        await notificationService.SendNotificationToUser(notifyUserRequest);
        
        return Ok();
    }
}
