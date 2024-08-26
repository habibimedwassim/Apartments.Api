using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RenTN.Application.DTOs.AuthDTOs;
using RenTN.Application.Services.EmailService;
using RenTN.Application.Utilities;
using RenTN.Domain.Common;
using RenTN.Domain.Entities;
using RenTN.Domain.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RenTN.Application.Services.Authentication;

public class AuthService(
    ILogger<AuthService> _logger,
    IEmailService _emailService,
    UserManager<User> _userManager,
    IOptions<JwtSettings> jwtSettings) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    public async Task<ApplicationResponse> RegisterAsync(RegisterDTO registerDTO)
    {
        var normalizedEmail = EmailNormalizer.NormalizeEmail(registerDTO.Email);
        var existingUser = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            return new ApplicationResponse(false, 400, $"User with this Email: {registerDTO.Email} exists already, login instead!");
        }

        var user = CreateUser(registerDTO);
        _logger.LogInformation("Creating User : {User}", user);

        user.LockoutEnabled = false;
        var result = await _userManager.CreateAsync(user, registerDTO.Password);

        if (result.Succeeded)
        {
            if (registerDTO.Role != null) 
            {
                await AssignRoleToUser(user, registerDTO.Role);
            }

            return await SendVerificationCodeAsync(user, VerificationCodeOperation.EmailVerification);
        }

        return new ApplicationResponse(false,StatusCodes.Status400BadRequest, string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<ApplicationResponse> LoginAsync(LoginDTO loginDTO)
    {
        var normalizedEmail = EmailNormalizer.NormalizeEmail(loginDTO.Email);
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDTO.Password))
        {
            throw new UnauthorizedAccessException("Invalid Email or Password!");
        }

        if (!user.EmailConfirmed)
        {
            var resendResult = await SendVerificationCodeAsync(user, VerificationCodeOperation.EmailVerification);
            throw new UnauthorizedAccessException("Email is not verified. A new verification code has been sent to your email.");
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
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Role = user.Role
        };

        return new ApplicationResponse(true, StatusCodes.Status200OK, "Login successful.", response);
    }

    public async Task<ApplicationResponse> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO)
    {
        var normalizedEmail = EmailNormalizer.NormalizeEmail(verifyEmailDTO.Email);
        var user = await _userManager.FindByEmailAsync(normalizedEmail) ?? 
                          throw new NotFoundException($"User with email : {verifyEmailDTO.Email} not found!");

        if (user.VerificationCode != verifyEmailDTO.VerificationCode || user.VerificationCodeExpiration < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired verification code.");
        }

        user.EmailConfirmed = true;
        user.VerificationCode = null;
        user.VerificationCodeExpiration = null;
        await _userManager.UpdateAsync(user);

        return new ApplicationResponse(true, StatusCodes.Status200OK, "Email verified successfully.");
    }

    public async Task<ApplicationResponse> ResendEmailAsync(EmailDTO email)
    {
        var normalizedEmail = EmailNormalizer.NormalizeEmail(email.Email);
        var user = await _userManager.FindByEmailAsync(normalizedEmail) ?? 
                   throw new NotFoundException($"User with email : {email.Email} not found!");

        return await SendVerificationCodeAsync(user, VerificationCodeOperation.EmailVerification);
    }

    public async Task<ApplicationResponse> ForgotPasswordAsync(EmailDTO email)
    {
        var normalizedEmail = EmailNormalizer.NormalizeEmail(email.Email);
        var user = await _userManager.FindByEmailAsync(normalizedEmail) ??
                   throw new NotFoundException($"User with email : {email.Email} not found!");

        if (!user.EmailConfirmed)
        {
            return await SendVerificationCodeAsync(user, VerificationCodeOperation.EmailVerification);
        }

        return await SendVerificationCodeAsync(user, VerificationCodeOperation.PasswordReset);
    }
    public async Task<ApplicationResponse> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO)
    {
        var normalizedEmail = EmailNormalizer.NormalizeEmail(resetPasswordDTO.Email);
        var user = await _userManager.FindByEmailAsync(normalizedEmail);
        if (user == null)
        {
            return new ApplicationResponse(false, StatusCodes.Status404NotFound, "Email not found."); 
        }

        if (user.VerificationCode != resetPasswordDTO.VerificationCode || user.VerificationCodeExpiration < DateTime.UtcNow)
        {
            return new ApplicationResponse(false, StatusCodes.Status401Unauthorized, "Invalid or expired verification code.");
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, resetPasswordDTO.NewPassword);

        if (!result.Succeeded)
        {
            return new ApplicationResponse(false, StatusCodes.Status400BadRequest, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        user.VerificationCode = null;
        user.VerificationCodeExpiration = null;
        await _userManager.UpdateAsync(user);

        return new ApplicationResponse(true, StatusCodes.Status200OK, "Password has been reset successfully.");
    }
    private async Task AssignRoleToUser(User user, string role)
    {
        switch (role) 
        { 
            case UserRoles.Admin:
            case UserRoles.Owner:
                user.Role = role;
                await _userManager.AddToRoleAsync(user, role);
                await _userManager.UpdateAsync(user);
                break;
        }
    }


    private User CreateUser(RegisterDTO registerDTO)
    {
        var normalizedEmail = EmailNormalizer.NormalizeEmail(registerDTO.Email);
        return new User
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FirstName = registerDTO.FirstName,
            LastName = registerDTO.LastName,
            PhoneNumber = registerDTO.PhoneNumber,
            Gender = registerDTO.Gender,
            DateOfBirth = registerDTO.DateOfBirth,
        };
    }
    private async Task<ApplicationResponse> SendVerificationCodeAsync(User user, VerificationCodeOperation context)
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

        return new ApplicationResponse(true, StatusCodes.Status200OK, $"{subject} has been sent to your email.");
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
            new Claim(JwtRegisteredClaimNames.Sub, user.Id!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.Gender, user.SysID.ToString())
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
