using Microsoft.AspNetCore.Mvc;
using RenTN.API.Utilities;
using RenTN.Application.DTOs.ChangeLogDTOs;
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
        var result = await _adminService.GetUsersAsync(UserRoles.Admin);
        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpGet("owners")]
    public async Task<IActionResult> GetActiveOwners()
    {
        var result = await _adminService.GetUsersAsync(UserRoles.Owner);
        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetActiveUsers()
    {
        var result = await _adminService.GetUsersAsync();
        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetActiveUsers([FromRoute] int id)
    {
        var result = await _adminService.GetUserByIdAsync(id);
        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpPost("changelogs")]
    public async Task<IActionResult> GetChangeLogs([FromBody] DateRangeDTO dateRange)
    {
        var result = await _adminService.GetChangeLogs(dateRange);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }
}
