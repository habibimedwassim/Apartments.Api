using Apartments.Application.Dtos.NotificationDtos;
using Apartments.Application.IServices;
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
}
