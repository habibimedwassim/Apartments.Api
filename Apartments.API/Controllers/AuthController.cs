using Apartments.Application.Common;
using Apartments.Application.Dtos.AuthDtos;
using Apartments.Application.IServices;
using Apartments.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apartments.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await authService.LoginAsync(loginDto);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ResultDetails(result.Message));
        }

        return Ok(result.Data);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        registerDto.Role = null;
        var result = await authService.RegisterAsync(registerDto);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("register-owner")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> RegisterOwner([FromBody] RegisterDto registerDto)
    {
        registerDto.Role = UserRoles.Owner;
        var result = await authService.RegisterAsync(registerDto);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("register-admin")]
    [Authorize(Roles = UserRoles.Admin)]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDto registerDto)
    {
        registerDto.Role = UserRoles.Admin;
        var result = await authService.RegisterAsync(registerDto);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDTO)
    {
        var result = await authService.VerifyEmailAsync(verifyEmailDTO);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("resend-code")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] EmailDto email)
    {
        var result = await authService.ResendEmailAsync(email);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] EmailDto email)
    {
        var result = await authService.ForgotPasswordAsync(email);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDTO)
    {
        var result = await authService.ResetPasswordAsync(resetPasswordDTO);

        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPatch("change-password")]
    [Authorize]
    public async Task<IActionResult> UpdateUserPassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var result = await authService.UpdateUserPassword(changePasswordDto);
        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }

    [HttpPatch("change-email")]
    [Authorize]
    public async Task<IActionResult> UpdateUserEmail([FromBody] EmailDto changeEmailDto)
    {
        var result = await authService.UpdateUserEmail(changeEmailDto);
        return StatusCode(result.StatusCode, new ResultDetails(result.Message));
    }
}
