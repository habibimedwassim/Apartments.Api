using Apartments.Application.Common;
using Apartments.Application.Dtos.AuthDtos;
using Apartments.Application.Dtos.UserDtos;
using Apartments.Application.IServices;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace Apartments.Application.Services;

public class AuthService(
    ILogger<AuthService> logger,
    IUserContext userContext,
    IEmailService emailService,
    IOptions<JwtSettings> jwtSettings,
    UserManager<User> userManager) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginDto loginDTO)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(loginDTO.Email);

        var user = await userManager.FindByEmailAsync(normalizedEmail);

        if (user == null || !await userManager.CheckPasswordAsync(user, loginDTO.Password))
        {
            logger.LogWarning("Login failed for email: {Email}. Invalid credentials.", loginDTO.Email);
            return ServiceResult<LoginResponseDto>.ErrorResult(StatusCodes.Status401Unauthorized, "Invalid email or password.");
        }

        if (user.IsDeleted) 
        {
            logger.LogInformation("User ({userEmail}) is disabled", loginDTO.Email);
            return ServiceResult<LoginResponseDto>.ErrorResult(StatusCodes.Status401Unauthorized, $"User ({loginDTO.Email}) is disabled");
        }

        if (!user.EmailConfirmed)
        {
            logger.LogWarning("Login attempt with unverified email: {Email}. Verification code sent.", loginDTO.Email);
            return ServiceResult<LoginResponseDto>.ErrorResult(StatusCodes.Status401Unauthorized, "Email is not verified. Please check your inbox!");
        }

        var accessToken = await GenerateAccessTokenAsync(user);
        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            Email = user.Email!,
            UserName = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Role = user.Role ?? UserRoles.User
        };

        return ServiceResult<LoginResponseDto>.SuccessResult(response, "Login successful.");
    }

    public async Task<ServiceResult<ResultDetails>> RegisterAsync(RegisterDto registerDto)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(registerDto.Email);
        var existingUser = await userManager.Users
                                            .FirstOrDefaultAsync(x => x.PhoneNumber == registerDto.PhoneNumber ||
                                                                      x.Email == normalizedEmail);
        if (existingUser != null)
        {
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status409Conflict, $"A user exists already.");
        }
        var user = CreateUser(registerDto);

        var result = await userManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(registerDto.Role))
            {
                await AssignRoleToUser(user, registerDto.Role);
            }

            await SendVerificationCodeAsync(user, VerificationCodeOperation.EmailVerification);
            logger.LogInformation("User {Email} created successfully. Verification email sent.", registerDto.Email);
            return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status201Created, "User created successfully. Please verify your email.");
        }

        logger.LogError("User creation failed for {Email}. Errors: {Errors}", registerDto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
        return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status400BadRequest, string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<ServiceResult<ResultDetails>> VerifyEmailAsync(VerifyEmailDto verifyEmailDTO)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(verifyEmailDTO.Email);
        var user = await userManager.FindByEmailAsync(normalizedEmail) ??
                   throw new NotFoundException($"User with email : {verifyEmailDTO.Email} not found!");

        if (user.VerificationCode != verifyEmailDTO.VerificationCode || user.VerificationCodeExpiration < DateTime.UtcNow)
        {
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status401Unauthorized, "Invalid or expired verification code.");
        }

        user.EmailConfirmed = true;
        user.VerificationCode = null;
        user.VerificationCodeExpiration = null;
        await userManager.UpdateAsync(user);

        return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK, "Email verified successfully.");
    }

    public async Task<ServiceResult<ResultDetails>> ResendEmailAsync(EmailDto email)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(email.Email);
        var user = await userManager.FindByEmailAsync(normalizedEmail) ??
                   throw new NotFoundException($"User with email : {email.Email} not found!");

        await SendVerificationCodeAsync(user, VerificationCodeOperation.VerificationCode);

        logger.LogInformation("Verification email resent to: {Email}", email.Email);
        return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK, "Verification code has been sent to your email.");
    }

    public async Task<ServiceResult<ResultDetails>> ForgotPasswordAsync(EmailDto email)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(email.Email);
        var user = await userManager.FindByEmailAsync(normalizedEmail) ??
                   throw new NotFoundException($"User with email : {email.Email} not found!");

        if (!user.EmailConfirmed)
        {
            logger.LogWarning("Forgot password failed for unverified email: {Email}. Verification code sent.", email.Email);
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status401Unauthorized, "Please verify your email before resetting your password.");
        }

        await SendVerificationCodeAsync(user, VerificationCodeOperation.PasswordReset);
        logger.LogInformation("Password reset code sent to: {Email}", email.Email);
        return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK, "Password reset code has been sent to your email.");
    }

    public async Task<ServiceResult<ResultDetails>> ResetPasswordAsync(ResetPasswordDto resetPasswordDTO)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(resetPasswordDTO.Email);
        var user = await userManager.FindByEmailAsync(normalizedEmail) ??
                   throw new NotFoundException($"User with email : {resetPasswordDTO.Email} not found!");

        if (user.VerificationCode != resetPasswordDTO.VerificationCode || user.VerificationCodeExpiration < DateTime.UtcNow)
        {
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status401Unauthorized, "Invalid or expired verification code.");
        }

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, resetPasswordDTO.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogError("Password reset failed for {Email}. Errors: {Errors}", resetPasswordDTO.Email, errors);
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status400BadRequest, errors);
        }

        user.VerificationCode = null;
        user.VerificationCodeExpiration = null;
        await userManager.UpdateAsync(user);

        return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK, "Password has been reset successfully.");
    }
    public async Task<ServiceResult<string>> UpdateUserPassword(ChangePasswordDto changePasswordDto)
    {
        var currentUser = userContext.GetCurrentUser();

        var user = await userManager.FindByIdAsync(currentUser.Id) ??
                   throw new NotFoundException($"User ({currentUser.Email}) not found!");

        if (!await userManager.CheckPasswordAsync(user, changePasswordDto.CurrentPassword))
        {
            return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest, "Invalid current password.");
        }

        var result = await userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Password update failed for {UserId}. Errors: {Errors}", user.Id, errors);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest, "Password update failed.");
        }

        logger.LogInformation("Password updated successfully for {UserId}.", user.Id);
        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Password updated successfully.");
    }
    public async Task<ServiceResult<string>> UpdateUserEmail(EmailDto changeEmailDto)
    {
        var currentUser = userContext.GetCurrentUser();

        var user = await userManager.FindByIdAsync(currentUser.Id) ??
                   throw new NotFoundException($"User ({currentUser.Email}) not found!");

        var normalizedEmail = CoreUtilities.NormalizeEmail(changeEmailDto.Email);

        if (await userManager.Users.AnyAsync(x => x.Email == normalizedEmail || x.UserName == normalizedEmail))
        {
            return ServiceResult<string>.ErrorResult(StatusCodes.Status409Conflict, $"User with Email ({changeEmailDto.Email}) already exists.");
        }

        try
        {
            await ChangeUserEmail(user, normalizedEmail, VerificationCodeOperation.EmailVerification);
            logger.LogInformation("User {Email} updated successfully. Verification email sent.", changeEmailDto.Email);
            return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Email updated successfully. Please verify it and login again.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update user email for {UserId}.", user.Id);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, "An error occurred while updating the email.");
        }
    }

    #region Helpers

    private async Task AssignRoleToUser(User user, string role)
    {
        if (role == UserRoles.Admin || role == UserRoles.Owner)
        {
            user.Role = role;
            await userManager.AddToRoleAsync(user, role);
            await userManager.UpdateAsync(user);
            logger.LogInformation("Assigned role {Role} to user {Email}.", role, user.Email);
        }
    }

    private User CreateUser(RegisterDto registerDTO)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(registerDTO.Email);
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

    private async Task SendVerificationCodeAsync(User user, VerificationCodeOperation context)
    {
        var verificationCode = GenerateVerificationCode();
        user.VerificationCode = verificationCode;
        user.VerificationCodeExpiration = DateTime.UtcNow.AddMinutes(AppConstants.CodeExpiration);
        await userManager.UpdateAsync(user);

        var subject = GetEmailSubject(context);
        string message = $"Your {subject.ToLower()} is: {verificationCode}";

        await emailService.SendEmailAsync(user.Email!, subject, message);
        logger.LogInformation("Sent {Context} code to {Email}.", subject, user.Email);
    }
    private async Task ChangeUserEmail(User user, string email, VerificationCodeOperation context)
    {
        var oldEmail = user.Email;

        // Generate a verification code and update user details
        var verificationCode = GenerateVerificationCode();
        user.VerificationCode = verificationCode;
        user.VerificationCodeExpiration = DateTime.UtcNow.AddMinutes(AppConstants.CodeExpiration);
        user.Email = email;
        user.UserName = email;
        user.NormalizedEmail = email.ToUpper();
        user.NormalizedUserName = email.ToUpper();
        user.EmailConfirmed = false;

        // Send the verification code to the new email
        var subject = GetEmailSubject(context);
        var message = $"Your email has been changed from {oldEmail} to {email}. Your verification code is: {verificationCode}";

        try
        {
            await emailService.SendEmailAsync(email, subject, message);
            logger.LogInformation("Sent {Context} code to {Email}.", subject, email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {Email}.", email);
            throw new InvalidOperationException("An error occurred while sending the verification email.");
        }
    }
    private string GetEmailSubject(VerificationCodeOperation context)
    {
        return context switch
        {
            VerificationCodeOperation.EmailVerification => "Email Verification Code",
            VerificationCodeOperation.PasswordReset => "Password Reset Code",
            _ => "Verification Code"
        };
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(1000, 9999).ToString();
    }

    private async Task<string> GenerateAccessTokenAsync(User user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Gender, user.SysId.ToString())
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

    #endregion
}
