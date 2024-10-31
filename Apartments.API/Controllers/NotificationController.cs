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
    [HttpGet]
    public async Task<IActionResult> GetUnreadNotifications()
    {
        var result = await notificationService.GetUnreadNotifications();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead([FromQuery] string type)
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
