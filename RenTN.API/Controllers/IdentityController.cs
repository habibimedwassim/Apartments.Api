using Microsoft.AspNetCore.Mvc;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Application.Services.IdentityService;

namespace RenTN.API.Controllers;

[ApiController]
[Route("api/identity")]
public class IdentityController(IIdentityService _identityService) : ControllerBase
{

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
    {
        var (success, response, message) = await _identityService.LoginAsync(loginDTO);

        if(!success) return BadRequest(message);

        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
    {
        var (success, message) = await _identityService.RegisterAsync(registerDTO);

        if (!success) return BadRequest(message);

        return Ok(message);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyEmailDTO)
    {
        var (success, message) = await _identityService.VerifyEmailAsync(verifyEmailDTO);

        if (!success) return BadRequest(message);

        return Ok(message);
    }

    [HttpPost("resend-code")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] EmailDTO email)
    {
        var (success, message) = await _identityService.ResendEmailAsync(email);

        if (!success) return BadRequest(message);

        return Ok(message);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] EmailDTO email)
    {
        var (success, message) = await _identityService.ForgotPasswordAsync(email);

        if (!success) return BadRequest(message);

        return Ok(message);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
    {
        var (success, message) = await _identityService.ResetPasswordAsync(resetPasswordDTO);

        if (!success) return BadRequest(message);

        return Ok(message);
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
