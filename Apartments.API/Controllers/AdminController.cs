using Apartments.Application.Common;
using Apartments.Application.Dtos.AdminDtos;
using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Application.Dtos.UserDtos;
using Apartments.Application.IServices;
using Apartments.Application.Services;
using Apartments.Domain.Common;
using Apartments.Domain.QueryFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartments.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminController(
    IAdminService adminService, 
    IDashboardService dashboardService) 
    : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var result = await dashboardService.GetAdminDashboard();

        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));

        return Ok(result.Data);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryFilter userQueryFilter)
    {
        var result = await adminService.GetAllUsers(userQueryFilter);

        return Ok(result);
    }

    [HttpPost("changelogs")]
    public async Task<IActionResult> GetChangeLogs([FromBody] ChangeLogDto changeLogDto, [FromQuery] ChangeLogQueryFilter filter)
    {
        var result = await adminService.GetChangeLogsPaged(changeLogDto, filter);
        return Ok(result);
    }

    [HttpGet("photos-cleanup")]
    public async Task<IActionResult> CleanupOrphanedPhotos()
    {
        var result = await adminService.CleanupAllOrphanedPhotosAsync();

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetUsersStatistics([FromQuery] string type)
    {
        var result = await adminService.GetStatistics(type);
        if (!result.Success) return StatusCode(result.StatusCode, new ResultDetails(result.Message));
        return Ok(result.Data);
    }

    [HttpPost("users/{id:int}/assign-role")]
    public async Task<IActionResult> AssignUserRole([FromRoute] int id, [FromBody] AssignRoleDto assignRoleDto)
    {
        assignRoleDto.UserId = id;
        var result = await adminService.AssignRole(assignRoleDto);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpDelete("users/{id:int}/unassign-role")]
    public async Task<IActionResult> UnAssignUserRole([FromRoute] int id)
    {
        var result = await adminService.UnAssignRole(id);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DisableUser([FromRoute] int id)
    {
        var result = await adminService.DisableUser(id);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpGet("users/{id:int}/restore")]
    public async Task<IActionResult> RestoreUser([FromRoute] int id)
    {
        var result = await adminService.RestoreUser(id);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }
}