using Apartments.Application.Common;
using Apartments.Application.Dtos.AuthDtos;
using Apartments.Application.Dtos.UserDtos;
using Apartments.Application.IServices;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Apartments.Application.Services;

public class AuthService(
    ILogger<AuthService> logger,
    IUserContext userContext,
    IEmailService emailService,
    IOptions<JwtSettings> jwtSettings,
    IUserRepository userRepository,
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
            return ServiceResult<LoginResponseDto>.ErrorResult(StatusCodes.Status401Unauthorized,
                "Invalid email or password.");
        }

        if (user.IsDeleted)
        {
            logger.LogInformation("User ({userEmail}) is disabled", loginDTO.Email);
            return ServiceResult<LoginResponseDto>.ErrorResult(StatusCodes.Status401Unauthorized,
                $"User ({loginDTO.Email}) is disabled");
        }

        if (!user.EmailConfirmed)
        {
            logger.LogWarning("Login attempt with unverified email: {Email}. Verification code sent.", loginDTO.Email);
            return ServiceResult<LoginResponseDto>.ErrorResult(StatusCodes.Status401Unauthorized,
                "Email is not verified. Please check your inbox!");
        }

        var accessToken = await GenerateAccessTokenAsync(user);
        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = CoreUtilities.ConstructUserFullName(user.FirstName, user.LastName),
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Role = user.Role ?? UserRoles.User
        };

        return ServiceResult<LoginResponseDto>.SuccessResult(response, "Login successful.");
    }

    public async Task<ServiceResult<ResultDetails>> RegisterAsync(RegisterDto registerDto)
    {
        using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            var normalizedEmail = CoreUtilities.NormalizeEmail(registerDto.Email);
            var existingUser = await userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == registerDto.PhoneNumber ||
                                          x.Email == normalizedEmail);
            if (existingUser != null)
                return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status409Conflict, $"A user exists already.");

            var user = CreateUser(registerDto);

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                var message = "Great to see you aboard! Let's quickly verify your email to get you started. " +
                    "Your verification code is: ";
                await SendVerificationCodeAsync(user, VerificationCodeOperation.EmailVerification, message, "Welcome!");
                logger.LogInformation("User {Email} created successfully. Verification email sent.", registerDto.Email);

                await transaction.CommitAsync();

                return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status201Created,
                    "User created successfully. Please verify your email.");
            }

            logger.LogError("User creation failed for {Email}. Errors: {Errors}", registerDto.Email,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            await transaction.RollbackAsync();

            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status400BadRequest,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        catch (Exception ex)
        {
            // Rollback transaction in case of exception
            await transaction.RollbackAsync();

            // Log exception
            logger.LogError(ex, "An error occurred while registering the user.");

            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError,
                "An error occurred during registration.");
        }
        
    }
    public async Task<ServiceResult<ResultDetails>> RegisterWithRoleAsync(RegisterDto registerDto, string role)
    {
        using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            var normalizedEmail = CoreUtilities.NormalizeEmail(registerDto.Email);
            var existingUser = await userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == registerDto.PhoneNumber ||
                                          x.Email == normalizedEmail);
            if (existingUser != null)
                return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status409Conflict, $"User exists already.");

            var user = CreateUserWithRole(registerDto, role);

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);

                await transaction.CommitAsync();

                return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status201Created,
                    $"{role} created successfully.");
            }

            logger.LogError("User creation failed for {Email}. Errors: {Errors}", registerDto.Email,
                string.Join(", ", result.Errors.Select(e => e.Description)));

            await transaction.RollbackAsync();

            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status400BadRequest,
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        catch (Exception ex) 
        {
            // Rollback transaction in case of exception
            await transaction.RollbackAsync();

            // Log exception
            logger.LogError(ex, "An error occurred while registering the user.");

            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError,
                "An error occurred during registration.");
        }
    }
    public async Task<ServiceResult<ResultDetails>> VerifyEmailAsync(VerifyEmailDto verifyEmailDTO)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(verifyEmailDTO.Email);
        var user = await userManager.FindByEmailAsync(normalizedEmail) ??
                   throw new NotFoundException($"User with email : {verifyEmailDTO.Email} not found!");

        if (user.EmailConfirmed)
        {
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status401Unauthorized,
                $"User with email: ({user.Email}) is already verified!");
        }

        if (user.VerificationCode != verifyEmailDTO.VerificationCode ||
            user.VerificationCodeExpiration < DateTime.UtcNow)
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status401Unauthorized,
                "Invalid or expired verification code.");

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

        if(user.VerificationCodeExpiration != null && user.VerificationCodeExpiration > DateTime.UtcNow)
        {
            var remainingSpan = user.VerificationCodeExpiration.Value - DateTime.UtcNow;
            var remainingMinutes = Math.Ceiling(remainingSpan.TotalMinutes);
            var message = $"Retry in {remainingMinutes} minutes";
            return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status401Unauthorized, message);
        }

        var resendMessage = "It looks like you requested a new verification code. " +
            "No worries, we've got you covered! Here's your new verification code:";
        await SendVerificationCodeAsync(user, VerificationCodeOperation.VerificationCode, resendMessage, "Verification Code");

        logger.LogInformation("Verification email resent to: {Email}", email.Email);
        return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK,
            "Verification code has been sent to your email.");
    }

    public async Task<ServiceResult<ResultDetails>> ForgotPasswordAsync(EmailDto email)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(email.Email);
        var user = await userManager.FindByEmailAsync(normalizedEmail) ??
                   throw new NotFoundException($"User with email : {email.Email} not found!");

        if (!user.EmailConfirmed)
        {
            logger.LogWarning("Forgot password failed for unverified email: {Email}. Verification code sent.",
                email.Email);
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status401Unauthorized,
                "Please verify your email before resetting your password.");
        }

        var message = "We're sorry to hear that you forgot your password! Your reset password code is:";
        await SendVerificationCodeAsync(user, VerificationCodeOperation.PasswordReset, message, "Password Reset");
        logger.LogInformation("Password reset code sent to: {Email}", email.Email);
        return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK,
            "Password reset code has been sent to your email.");
    }

    public async Task<ServiceResult<ResultDetails>> ResetPasswordAsync(ResetPasswordDto resetPasswordDTO)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(resetPasswordDTO.Email);
        var user = await userManager.FindByEmailAsync(normalizedEmail) ??
                   throw new NotFoundException($"User with email : {resetPasswordDTO.Email} not found!");

        if (user.VerificationCode != resetPasswordDTO.VerificationCode ||
            user.VerificationCodeExpiration < DateTime.UtcNow)
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status401Unauthorized,
                "Invalid or expired verification code.");

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

        return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK,
            "Password has been reset successfully.");
    }

    public async Task<ServiceResult<string>> UpdateUserPassword(ChangePasswordDto changePasswordDto)
    {
        var currentUser = userContext.GetCurrentUser();

        var user = await userManager.FindByIdAsync(currentUser.Id) ??
                   throw new NotFoundException($"User ({currentUser.Email}) not found!");

        if (!await userManager.CheckPasswordAsync(user, changePasswordDto.CurrentPassword))
            return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest, "Invalid current password.");

        var result =
            await userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword);
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
            return ServiceResult<string>.ErrorResult(StatusCodes.Status409Conflict,
                $"User with Email ({changeEmailDto.Email}) already exists.");

        try
        {
            await ChangeUserEmail(user, normalizedEmail, VerificationCodeOperation.EmailVerification);
            logger.LogInformation("User {Email} updated successfully. Verification email sent.", changeEmailDto.Email);
            return ServiceResult<string>.InfoResult(StatusCodes.Status200OK,
                "Email updated successfully. Please verify it and login again.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update user email for {UserId}.", user.Id);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError,
                "An error occurred while updating the email.");
        }
    }

    #region Helpers

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
            Role = UserRoles.User,
        };
    }
    private User CreateUserWithRole(RegisterDto registerDTO, string role)
    {
        if (role == UserRoles.Admin || role == UserRoles.Owner)
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
                Role = role,
                EmailConfirmed = true
            };
        }
        throw new BadRequestException("User role should be Admin or Owner");
            
    }
    //private async Task SendVerificationCodeAsync(User user, VerificationCodeOperation context)
    //{
    //    var verificationCode = GenerateVerificationCode();
    //    user.VerificationCode = verificationCode;
    //    user.VerificationCodeExpiration = DateTime.UtcNow.AddMinutes(AppConstants.CodeExpiration);
    //    await userManager.UpdateAsync(user);

    //    var subject = GetEmailSubject(context);
    //    var message = $"Your {subject.ToLower()} is: {verificationCode}";

    //    await emailService.SendEmailAsync(user.Email!, subject, message);
    //    logger.LogInformation("Sent {Context} code to {Email}.", subject, user.Email);
    //}
    private async Task SendVerificationCodeAsync(User user, VerificationCodeOperation context, string message, string title)
    {
        // Generate the verification code
        var verificationCode = GenerateVerificationCode();
        user.VerificationCode = verificationCode;
        user.VerificationCodeExpiration = DateTime.UtcNow.AddMinutes(AppConstants.CodeExpiration);
        await userManager.UpdateAsync(user);

        var userName = user.FirstName ?? "";

        // Get the email subject
        var subject = GetEmailSubject(context);

        // Create placeholders for the email template
        var placeholders = new Dictionary<string, string>
        {
            { "Title", title },
            { "UserName", userName },
            { "Message", message },
            { "VerificationCode", verificationCode },
            { "ExpirationTime", AppConstants.CodeExpiration.ToString() }
        };

        // Load and populate the email template with placeholders
        var emailBody = await emailService.GetEmailTemplateAsync("VerificationEmailTemplate", placeholders);

        // Send the email with the populated template
        await emailService.SendEmailAsync(user.Email!, subject, emailBody);

        // Log the event
        logger.LogInformation("Sent {Context} code to {Email}.", subject, user.Email);
    }
    private string GetEmailSubject(VerificationCodeOperation context)
    {
        return context switch
        {
            VerificationCodeOperation.EmailVerification => "Let's verify your email",
            VerificationCodeOperation.PasswordReset => "Let's reset your password",
            _ => "Verification Code"
        };
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
        var message =
            $"Your email has been changed from {oldEmail} to {email}. Your verification code is: {verificationCode}";

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

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    private async Task<string> GenerateAccessTokenAsync(User user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id!),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Gender, user.SysId.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            expires: DateTime.UtcNow.AddDays(AppConstants.TokenExpiration),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    #endregion
}