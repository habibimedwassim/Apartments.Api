using Microsoft.AspNetCore.Mvc;
using RenTN.Application.DTOs.AuthDTOs;
using RenTN.Application.Services.Authentication;
using RenTN.Domain.Common;

namespace RenTN.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService _authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginDTO loginDTO)
    {
        var (success, response, message) = await _authService.LoginAsync(loginDTO);

        if (!success) return BadRequest(message);

        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
    {
        registerDTO.Role = null;
        var (success, message) = await _authService.RegisterAsync(registerDTO);

        if (!success) return BadRequest(message);

        return Ok(message);
    }

    [HttpPost("register-owner")]
    public async Task<IActionResult> RegisterOwner([FromBody] RegisterDTO registerDTO)
    {
        registerDTO.Role = UserRoles.Owner;
        var (success, message) = await _authService.RegisterAsync(registerDTO);

        if (!success) return BadRequest(message);

        return Ok(message);
    }

    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDTO registerDTO)
    {
        registerDTO.Role = UserRoles.Admin;
        var (success, message) = await _authService.RegisterAsync(registerDTO);

        if (!success) return BadRequest(message);

        return Ok(message);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyEmailDTO)
    {
        var (success, message) = await _authService.VerifyEmailAsync(verifyEmailDTO);

        if (!success) return BadRequest(message);

        return Ok(message);
    }

    [HttpPost("resend-code")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] EmailDTO email)
    {
        var (success, message) = await _authService.ResendEmailAsync(email);

        if (!success) return BadRequest(message);

        return Ok(message);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] EmailDTO email)
    {
        var (success, message) = await _authService.ForgotPasswordAsync(email);

        if (!success) return BadRequest(message);

        return Ok(message);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
    {
        var (success, message) = await _authService.ResetPasswordAsync(resetPasswordDTO);

        if (!success) return BadRequest(message);

        return Ok(message);
    }
}
