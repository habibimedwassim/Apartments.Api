using Microsoft.AspNetCore.Mvc;
using RenTN.API.Utilities;
using RenTN.Application.DTOs.AuthDTOs;
using RenTN.Application.Services.Authentication;
using RenTN.Domain.Common;

namespace RenTN.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService _authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
    {
        var result = await _authService.LoginAsync(loginDTO);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
    {
        registerDTO.Role = null;
        var result = await _authService.RegisterAsync(registerDTO);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpPost("register-owner")]
    public async Task<IActionResult> RegisterOwner([FromBody] RegisterDTO registerDTO)
    {
        registerDTO.Role = UserRoles.Owner;
        var result = await _authService.RegisterAsync(registerDTO);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDTO registerDTO)
    {
        registerDTO.Role = UserRoles.Admin;
        var result = await _authService.RegisterAsync(registerDTO);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyEmailDTO)
    {
        var result = await _authService.VerifyEmailAsync(verifyEmailDTO);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpPost("resend-code")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] EmailDTO email)
    {
        var result = await _authService.ResendEmailAsync(email);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] EmailDTO email)
    {
        var result = await _authService.ForgotPasswordAsync(email);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
    {
        var result = await _authService.ResetPasswordAsync(resetPasswordDTO);

        if (!result.Success) return ApiResponse.Error(result);

        return ApiResponse.Success(result);
    }
}
