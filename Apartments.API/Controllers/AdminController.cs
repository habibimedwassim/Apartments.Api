using Apartments.Application.Common;
using Apartments.Application.Dtos.AdminDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartments.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = UserRoles.Admin)]
public class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpPost("changelogs")]
    public async Task<IActionResult> GetChangeLogs([FromBody] ChangeLogDto changeLogDto)
    {
        var result = await adminService.GetChangeLogs(changeLogDto);

        return Ok(result.Data);
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