using Microsoft.AspNetCore.Mvc;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Application.Services.IdentityService;

namespace RenTN.API.Controllers;

[ApiController]
[Route("api/identity")]
public class IdentityController(IIdentityService _identityService) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<ActionResult> GetUserProfile()
    {
        var profile = await _identityService.GetCurrentUserProfile();
        return Ok(profile);
    }

    [HttpPost("userRole")]
    public async Task<IActionResult> AssignUserRole([FromBody] AssignRoleDTO assignRoleDTO)
    {
        await _identityService.AssignRole(assignRoleDTO);
        return NoContent();
    }

    [HttpDelete("userRole")]
    public async Task<IActionResult> UnassignUserRole([FromBody] AssignRoleDTO unassignRoleDTO)
    {
        await _identityService.UnassignRole(unassignRoleDTO);
        return NoContent();
    }
}
