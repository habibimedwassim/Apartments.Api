using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Application.Services.EmailService;
using RenTN.Domain.Common;
using RenTN.Domain.Entities;
using RenTN.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RenTN.Application.Services.IdentityService;

public class IdentityService(
    ILogger<IdentityService> _logger,
    IEmailService _emailService,
    UserManager<User> _userManager,
    RoleManager<IdentityRole> _roleManager,
    IOptions<JwtSettings> jwtSettings) : IIdentityService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterDTO registerDTO)
    {
        var user = new User
        {
            UserName = registerDTO.UserName,
            Email = registerDTO.Email,
            FirstName = registerDTO.FirstName,
            LastName = registerDTO.LastName
        };

        var result = await _userManager.CreateAsync(user, registerDTO.Password);

        if (result.Succeeded)
        {
            return await SendVerificationCodeAsync(user, VerificationCodeOperation.EmailVerification);
        }

        return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<(bool Success, AuthResponse? Response, string Message)> LoginAsync(LoginDTO loginDTO)
    {
        var user = await _userManager.FindByNameAsync(loginDTO.UserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDTO.Password))
        {
            return (false, null, "Invalid username or password.");
        }

        if (!user.EmailConfirmed)
        {
            var resendResult = await SendVerificationCodeAsync(user, VerificationCodeOperation.EmailVerification);
            return (false, null, "Email is not verified. A new verification code has been sent to your email.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = await GenerateAccessTokenAsync(user);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            Email = user.Email!,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles
        };

        return (true, response, "Login successful.");
    }

    public async Task<(bool Success, string Message)> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO)
    {
        var user = await _userManager.FindByEmailAsync(verifyEmailDTO.Email);
        if (user == null)
        {
            return (false, "Invalid email.");
        }

        if (user.VerificationCode != verifyEmailDTO.VerificationCode || user.VerificationCodeExpiration < DateTime.UtcNow)
        {
            return (false, "Invalid or expired verification code.");
        }

        user.EmailConfirmed = true;
        user.VerificationCode = null;
        user.VerificationCodeExpiration = null;
        await _userManager.UpdateAsync(user);

        return (true, "Email verified successfully.");
    }

    public async Task<(bool success, string message)> ResendEmailAsync(EmailDTO email)
    {
        var user = await _userManager.FindByEmailAsync(email.Email);
        if (user == null)
        {
            return (false, "Email not found.");
        }

        return await SendVerificationCodeAsync(user, VerificationCodeOperation.EmailVerification);
    }

    public async Task<(bool success, string message)> ForgotPasswordAsync(EmailDTO email)
    {
        var user = await _userManager.FindByEmailAsync(email.Email);
        if (user == null)
        {
            return (false, "Email not found.");
        }

        if (!user.EmailConfirmed)
        {
            return await SendVerificationCodeAsync(user, VerificationCodeOperation.EmailVerification);
        }

        return await SendVerificationCodeAsync(user, VerificationCodeOperation.PasswordReset);
    }
    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordDTO.Email);
        if (user == null)
        {
            return (false, "Email not found.");
        }

        if (user.VerificationCode != resetPasswordDTO.VerificationCode || user.VerificationCodeExpiration < DateTime.UtcNow)
        {
            return (false, "Invalid or expired verification code.");
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, resetPasswordDTO.NewPassword);

        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        user.VerificationCode = null;
        user.VerificationCodeExpiration = null;
        await _userManager.UpdateAsync(user);

        return (true, "Password has been reset successfully.");
    }
    public async Task AssignRole(AssignRoleDTO assignRoleDTO)
    {
        _logger.LogInformation("Assigning user role: {@Request}", assignRoleDTO);

        var user = await _userManager.FindByEmailAsync(assignRoleDTO.UserEmail) ??
                   throw new NotFoundException(nameof(User), assignRoleDTO.UserEmail);

        var role = await _roleManager.FindByNameAsync(assignRoleDTO.RoleName) ??
                   throw new NotFoundException(nameof(IdentityRole), assignRoleDTO.RoleName);

        await _userManager.AddToRoleAsync(user, role.Name!);
    }

    public async Task UnassignRole(AssignRoleDTO unassignRoleDTO)
    {
        _logger.LogInformation("Unassigning user role: {@Request}", unassignRoleDTO);

        var user = await _userManager.FindByEmailAsync(unassignRoleDTO.UserEmail) ??
                   throw new NotFoundException(nameof(User), unassignRoleDTO.UserEmail);

        var role = await _roleManager.FindByNameAsync(unassignRoleDTO.RoleName) ??
                   throw new NotFoundException(nameof(IdentityRole), unassignRoleDTO.RoleName);

        await _userManager.RemoveFromRoleAsync(user, role.Name!);
    }

    private async Task<(bool success, string message)> SendVerificationCodeAsync(User user, VerificationCodeOperation context)
    {
        var verificationCode = GenerateVerificationCode();
        user.VerificationCode = verificationCode;
        user.VerificationCodeExpiration = DateTime.UtcNow.AddMinutes(Constants.CodeExpiration);
        await _userManager.UpdateAsync(user);

        string subject = context switch
        {
            VerificationCodeOperation.EmailVerification => "Email Verification Code",
            VerificationCodeOperation.PasswordReset => "Password Reset Code",
            _ => "Verification Code"
        };

        string message = $"Your {subject.ToLower()} is: {verificationCode}";

        await _emailService.SendEmailAsync(user.Email!, subject, message);

        return (true, $"{subject} has been sent to your email.");
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(1000, 9999).ToString();
    }

    private async Task<string> GenerateAccessTokenAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
