using Microsoft.AspNetCore.Mvc;
using RenTN.Application.Services.AdminService;
using RenTN.Domain.Common;

namespace RenTN.API.Controllers;
[ApiController]
[Route("api/admin")]
public class AdminController(IAdminService _adminService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActiveAdmins()
    {
        var admins = await _adminService.GetUsersAsync(UserRoles.Admin);
        return Ok(admins);
    }

    [HttpGet("owners")]
    public async Task<IActionResult> GetActiveOwners()
    {
        var owners = await _adminService.GetUsersAsync(UserRoles.Owner);
        return Ok(owners);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetActiveUsers()
    {
        var users = await _adminService.GetUsersAsync();
        return Ok(users);
    }
}
