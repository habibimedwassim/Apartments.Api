using Apartments.Application.Common;
using Apartments.Application.Dtos.AuthDtos;
using Apartments.Application.Dtos.EmailDtos;
using Apartments.Application.IServices;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
                .FirstOrDefaultAsync(x => x.PhoneNumber == registerDto.PhoneNumber || x.Email == normalizedEmail);

            if (existingUser != null)
            {
                var existsMessage = normalizedEmail == existingUser.Email
                    ? $"A User with email {registerDto.Email} exists already!"
                    : $"A User with phone number {registerDto.PhoneNumber} exists already!";

                return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status409Conflict, existsMessage);
            }

            var user = CreateUser(registerDto);
            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                logger.LogError("User creation failed for {Email}. Errors: {Errors}", registerDto.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                await transaction.RollbackAsync();
                return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status400BadRequest,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            var verificationRequest = new VerificationRequest
            {
                User = user,
                CodeType = VerificationCodeType.Email,
                Title = "Welcome!",
                Message = "Your verification code is: "
            };

            await SendVerificationCodeAsync(verificationRequest);
            logger.LogInformation("User {Email} created successfully. Verification email sent.", registerDto.Email);

            await transaction.CommitAsync();
            return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status201Created, "User created successfully. Please verify your email.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "An error occurred during registration.");
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError, "An error occurred during registration.");
        }
    }


    public async Task<ServiceResult<ResultDetails>> RegisterWithRoleAsync(RegisterDto registerDto, string role)
    {
        using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            var normalizedEmail = CoreUtilities.NormalizeEmail(registerDto.Email);
            var existingUser = await userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == registerDto.PhoneNumber || x.Email == normalizedEmail);

            if (existingUser != null)
            {
                var existsMessage = normalizedEmail == existingUser.Email
                    ? $"A User with email {registerDto.Email} exists already!"
                    : $"A User with phone number {registerDto.PhoneNumber} exists already!";

                return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status409Conflict, existsMessage);
            }

            var user = CreateUserWithRole(registerDto, role);
            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                logger.LogError("User creation failed for {Email}. Errors: {Errors}", registerDto.Email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                await transaction.RollbackAsync();
                return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status400BadRequest,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(user, role);
            logger.LogInformation("{Role} {Email} created successfully.", role, registerDto.Email);

            await transaction.CommitAsync();
            return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status201Created, $"{role} created successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "An error occurred during registration.");
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError, "An error occurred during registration.");
        }
    }


    public async Task<ServiceResult<ResultDetails>> ResendEmailAsync(EmailDto email, string type)
    {
        using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            // Validate type
            var verificationCodeType = CoreUtilities.ValidateEnum<VerificationCodeType>(type);
            var normalizedEmail = CoreUtilities.NormalizeEmail(email.Email);

            // Find user
            var user = await userManager.FindByEmailAsync(normalizedEmail)
                       ?? throw new NotFoundException($"User with email : {email.Email} not found!");

            // Check if code is still valid
            if (IsCodeStillValid(user, verificationCodeType))
            {
                var remainingMinutes = GetRemainingMinutes(user, verificationCodeType);
                return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status401Unauthorized,
                    $"Retry in {remainingMinutes} minutes");
            }

            // Create verification request
            var verificationRequest = new VerificationRequest
            {
                User = user,
                CodeType = verificationCodeType,
                Title = verificationCodeType == VerificationCodeType.Password ? "Password Reset" : "Verification Code",
                Message = "Your new verification code:"
            };

            // Try to send verification code
            var emailSent = await SendVerificationCodeAsync(verificationRequest);
            if (!emailSent)
            {
                await transaction.RollbackAsync();
                return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError,
                    "Failed to send verification email. Please try again.");
            }

            // Commit transaction if email sent successfully
            await transaction.CommitAsync();
            logger.LogInformation("Verification email resent to: {Email}", email.Email);

            return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK,
                "Verification code has been sent to your email.");
        }
        catch (Exception ex)
        {
            // Rollback and log any exceptions
            await transaction.RollbackAsync();
            logger.LogError(ex, "An error occurred while resending the verification code.");
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError,
                "An error occurred while resending the verification code.");
        }
    }

    public async Task<ServiceResult<ResultDetails>> VerifyEmailAsync(VerifyEmailDto verifyEmailDTO)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(verifyEmailDTO.Email);
        var user = await userManager.Users
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail || x.TempEmail == normalizedEmail)
            ?? throw new NotFoundException($"User with email : {verifyEmailDTO.Email} not found!");

        // If verifying the primary email (regular flow)
        if (user.Email == normalizedEmail && user.EmailConfirmed)
        {
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status401Unauthorized,
                $"User with email: ({user.Email}) is already verified!");
        }

        // If verifying the temporary email (email change flow)
        if (user.TempEmail == normalizedEmail && user.TempEmailConfirmed)
        {
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status401Unauthorized,
                $"Temporary email: ({user.TempEmail}) is already verified!");
        }

        // Check the code and expiration
        if (user.EmailCode != verifyEmailDTO.VerificationCode || user.EmailCodeExpiration < DateTime.UtcNow)
        {
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status401Unauthorized,
                "Invalid or expired verification code.");
        }

        // If it's a primary email verification
        if (user.Email == normalizedEmail)
        {
            user.EmailConfirmed = true;
        }
        // If it's a temporary email verification
        else if (user.TempEmail == normalizedEmail)
        {
            user.Email = normalizedEmail;
            user.EmailConfirmed = true;
            user.TempEmail = null;
            user.TempEmailConfirmed = false;
        }

        // Clear the code and expiration fields
        user.EmailCode = null;
        user.EmailCodeExpiration = null;
        user.UserName = normalizedEmail;
        user.NormalizedUserName = normalizedEmail.ToUpper();

        await userManager.UpdateAsync(user);

        return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK, "Email verified successfully.");
    }



    public async Task<ServiceResult<ResultDetails>> ForgotPasswordAsync(EmailDto email)
    {
        using var transaction = await userRepository.BeginTransactionAsync();
        try
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

            if (IsCodeStillValid(user, VerificationCodeType.Password))
            {
                var remainingMinutes = GetRemainingMinutes(user, VerificationCodeType.Password);
                var retryMessage = $"Retry in {remainingMinutes} minutes";
                return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status401Unauthorized, retryMessage);
            }

            var message = "Your reset password code is:";
            var verificationRequest = new VerificationRequest
            {
                User = user,
                CodeType = VerificationCodeType.Password,
                Title = "Password Reset",
                Message = message
            };

            if (await SendVerificationCodeAsync(verificationRequest))
            {
                await transaction.CommitAsync();
                logger.LogInformation("Password reset code sent to: {Email}", email.Email);
                return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK,
                    "Password reset code has been sent to your email.");
            }

            // If email sending fails
            await transaction.RollbackAsync();
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError,
                "Failed to send the reset password code. Please try again.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "An error occurred while sending the reset password code.");
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError,
                "An error occurred during the forgot password process.");
        }
    }


    public async Task<ServiceResult<ResultDetails>> ResetPasswordAsync(ResetPasswordDto resetPasswordDTO)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(resetPasswordDTO.Email);
        var user = await userManager.FindByEmailAsync(normalizedEmail) ??
                   throw new NotFoundException($"User with email : {resetPasswordDTO.Email} not found!");

        if (user.ResetCode != resetPasswordDTO.VerificationCode ||
            user.ResetCodeExpiration < DateTime.UtcNow)
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

        user.ResetCode = null;
        user.ResetCodeExpiration = null;
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

    public async Task<ServiceResult<string>> UpdateUserEmail(UpdateEmailDto updateEmailDto)
    {
        using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            var currentUser = userContext.GetCurrentUser();
            var user = await userManager.FindByIdAsync(currentUser.Id) ??
                       throw new NotFoundException($"User ({currentUser.Email}) not found!");

            // Ensure the user provides their current password
            if (!await userManager.CheckPasswordAsync(user, updateEmailDto.CurrentPassword))
            {
                return ServiceResult<string>.ErrorResult(StatusCodes.Status401Unauthorized, "Invalid current password.");
            }

            var normalizedEmail = CoreUtilities.NormalizeEmail(updateEmailDto.Email);

            // Ensure no other user has the new email
            if (await userManager.Users.AnyAsync(x => x.Email == normalizedEmail || x.TempEmail == normalizedEmail))
            {
                return ServiceResult<string>.ErrorResult(StatusCodes.Status409Conflict, $"Email ({updateEmailDto.Email}) already exists.");
            }

            // Send verification code to the new email
            var verificationRequest = new VerificationRequest
            {
                User = user,
                CodeType = VerificationCodeType.Email,
                Title = "Verify your new email",
                Message = "Please verify your new email address using the code below:"
            };

            // If the email was sent successfully, update the user with temporary email
            if (await SendEmailChangeVerificationCodeAsync(verificationRequest, updateEmailDto.Email))
            {
                user.TempEmail = normalizedEmail;
                user.EmailCode = verificationRequest.User.EmailCode;
                user.EmailCodeExpiration = DateTime.UtcNow.AddMinutes(AppConstants.CodeExpiration);
                user.TempEmailConfirmed = false;

                await userManager.UpdateAsync(user);
                await transaction.CommitAsync();

                logger.LogInformation("Verification email sent to: {TempEmail}", user.TempEmail);
                return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Verification code sent to the new email. Please verify.");
            }

            await transaction.RollbackAsync();
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, "Failed to send verification email.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error occurred while updating email.");
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, "An error occurred while updating the email.");
        }
    }

    private async Task<bool> SendEmailChangeVerificationCodeAsync(VerificationRequest request, string tempEmail)
    {
        try
        {
            // Generate verification code and expiration
            request.User.EmailCode = GenerateVerificationCode();
            request.User.EmailCodeExpiration = DateTime.UtcNow.AddMinutes(AppConstants.CodeExpiration);

            // Prepare placeholders for the email sent to the new email
            var newEmailPlaceholders = new Dictionary<string, string>
            {
                { "Title", "Verify your new email" },
                { "UserName", request.User.FirstName ?? "" },
                { "Message", request.Message },
                { "VerificationCode", request.User.EmailCode },
                { "ExpirationTime", AppConstants.CodeExpiration.ToString() }
            };

            // Prepare placeholders for the email sent to the old email
            var oldEmailPlaceholders = new Dictionary<string, string>
            {
                { "Title", "Your email has been updated" },
                { "UserName", request.User.FirstName ?? "" },
                { "NotificationDetails", "Your email has been changed to " + tempEmail + ". If this wasn't you, please contact support immediately." }
            };

            // Send the verification email to the new email
            var emailBodyNewEmail = await emailService.GetEmailTemplateAsync("VerificationEmailTemplate", newEmailPlaceholders);
            var subjectNewEmail = "Verify your email change";
            await emailService.SendEmailAsync(tempEmail, subjectNewEmail, emailBodyNewEmail);

            // Send notification to the old email about the change
            var emailBodyOldEmail = await emailService.GetEmailTemplateAsync("EmailNotificationTemplate", oldEmailPlaceholders);
            var subjectOldEmail = "Email Change Notification";
            await emailService.SendEmailAsync(request.User.Email!, subjectOldEmail, emailBodyOldEmail);

            // Log and commit the transaction after successful email sending
            logger.LogInformation("Verification email sent to new address: {TempEmail} and notification sent to old email: {OldEmail}.", tempEmail, request.User.Email);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification or notification emails during email change.");
            return false;
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

    private async Task<bool> SendVerificationCodeAsync(VerificationRequest request)
    {
        try
        {
            // Generate verification code and expiration
            var verificationCode = GenerateVerificationCode();
            var codeExpiration = DateTime.UtcNow.AddMinutes(AppConstants.CodeExpiration);

            // Prepare placeholders for the email template
            var placeholders = new Dictionary<string, string>
            {
                { "Title", request.Title },
                { "UserName", request.User.FirstName ?? "" },
                { "Message", request.Message },
                { "VerificationCode", verificationCode },
                { "ExpirationTime", AppConstants.CodeExpiration.ToString() }
            };

            // Populate the email template
            var emailBody = await emailService.GetEmailTemplateAsync("VerificationEmailTemplate", placeholders);

            // Send the email
            var context = request.CodeType == VerificationCodeType.Password ? VerificationCodeOperation.PasswordReset
                                                                             : VerificationCodeOperation.EmailVerification;
            var subject = GetEmailSubject(context);
            await emailService.SendEmailAsync(request.User.Email!, subject, emailBody);

            // If email is sent successfully, update the user with the verification code
            if (request.CodeType == VerificationCodeType.Email)
            {
                request.User.EmailCode = verificationCode;
                request.User.EmailCodeExpiration = codeExpiration;
            }
            else if (request.CodeType == VerificationCodeType.Password)
            {
                request.User.ResetCode = verificationCode;
                request.User.ResetCodeExpiration = codeExpiration;
            }

            await userManager.UpdateAsync(request.User);

            logger.LogInformation("Sent {Context} code to {Email}.", subject, request.User.Email);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification email to {Email}.", request.User.Email);
            return false;
        }
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
    private bool IsCodeStillValid(User user, VerificationCodeType verificationCodeType)
    {
        return verificationCodeType == VerificationCodeType.Email &&
               user.EmailCodeExpiration > DateTime.UtcNow ||
               verificationCodeType == VerificationCodeType.Password &&
               user.ResetCodeExpiration > DateTime.UtcNow;
    }

    private double GetRemainingMinutes(User user, VerificationCodeType verificationCodeType)
    {
        var expiration = verificationCodeType == VerificationCodeType.Email
                         ? user.EmailCodeExpiration
                         : user.ResetCodeExpiration;

        if (expiration == null) return 0;
        
        return Math.Ceiling((expiration.Value - DateTime.UtcNow).TotalMinutes);
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