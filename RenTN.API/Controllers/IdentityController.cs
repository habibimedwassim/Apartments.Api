using Microsoft.AspNetCore.Mvc;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Application.Services.IdentityService;

namespace RenTN.API.Controllers;

[ApiController]
[Route("api/identity")]
public class IdentityController(IIdentityService _identityService) : ControllerBase
{
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
