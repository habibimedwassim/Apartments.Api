using Apartments.Application.Common;
using Apartments.Application.Dtos.AuthDtos;
using Apartments.Application.Dtos.EmailDtos;
using Apartments.Application.IServices;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using AutoMapper;
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
                                          x.Email == normalizedEmail ||
                                          x.CIN == registerDto.CIN);

            if (existingUser != null)
            {
                var existsMessage = normalizedEmail == existingUser.Email
                                    ? $"A User with email {registerDto.Email} exists already!"
                                    : registerDto.PhoneNumber == existingUser.PhoneNumber
                                        ? $"A User with phone number {registerDto.PhoneNumber} exists already!"
                                        : registerDto.CIN == existingUser.CIN
                                            ? $"A User with CIN {registerDto.CIN} exists already!"
                                            : "A user with similar details exists.";

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
                .FirstOrDefaultAsync(x => x.PhoneNumber == registerDto.PhoneNumber ||
                                          x.Email == normalizedEmail ||
                                          x.CIN == registerDto.CIN);

            if (existingUser != null)
            {
                var existsMessage = normalizedEmail == existingUser.Email
                                    ? $"A User with email {registerDto.Email} exists already!"
                                    : registerDto.PhoneNumber == existingUser.PhoneNumber
                                        ? $"A User with phone number {registerDto.PhoneNumber} exists already!"
                                        : registerDto.CIN == existingUser.CIN
                                            ? $"A User with CIN {registerDto.CIN} exists already!"
                                            : "A user with similar details exists.";

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

        if (user.EmailCode != verifyEmailDTO.VerificationCode ||
            user.EmailCodeExpiration < DateTime.UtcNow)
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status401Unauthorized,
                "Invalid or expired verification code.");

        user.EmailConfirmed = true;
        user.EmailCode = null;
        user.EmailCodeExpiration = null;
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

            var remainingMinutes = GetRemainingMinutes(user, VerificationCodeType.Password);
            if (remainingMinutes < 0)
            {
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
        catch (NotFoundException ex)
        {
            logger.LogError(ex, "User not found: {Email}", email.Email);
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status404NotFound, ex.Message);
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
    public async Task<ServiceResult<ResultDetails>> ResendVerificationCodeAsync(EmailDto email, string type)
    {
        // Parse and validate the type of verification code requested
        var verificationCodeType = CoreUtilities.ValidateEnum<VerificationCodeType>(type);
        var normalizedEmail = CoreUtilities.NormalizeEmail(email.Email);

        // Delegate the request based on verification code type
        return verificationCodeType switch
        {
            VerificationCodeType.Password => await HandleResetCodeAsync(normalizedEmail),
            VerificationCodeType.Email or VerificationCodeType.NewEmail => await HandleEmailCodeAsync(normalizedEmail, verificationCodeType),
            _ => ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status400BadRequest, "Invalid verification type.")
        };
    }

    #region Helpers
    private async Task<ServiceResult<ResultDetails>> HandleResetCodeAsync(string email)
    {
        using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            // Fetch user by email
            var user = await userManager.FindByEmailAsync(email) ??
                       throw new NotFoundException($"User with email : {email} not found!");

            // Check if the reset code is still valid
            var remainingMinutes = GetRemainingMinutes(user, VerificationCodeType.Password);
            if (remainingMinutes > 0)
            {
                return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status401Unauthorized,
                    $"Retry in {remainingMinutes} minutes.");
            }

            // Prepare request and send reset code
            var verificationRequest = new VerificationRequest
            {
                User = user,
                CodeType = VerificationCodeType.Password,
                Title = "Password Reset",
                Message = "Your new password reset code:"
            };

            if (await SendVerificationCodeAsync(verificationRequest))
            {
                await transaction.CommitAsync();
                logger.LogInformation("Password reset code resent to email: {Email}", user.Email);
                return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK,
                    "Password reset code has been resent to your email.");
            }

            await transaction.RollbackAsync();
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError,
                "Failed to resend password reset code.");
        }
        catch (NotFoundException ex)
        {
            logger.LogError(ex, "User not found: {Email}", email);
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status404NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error occurred while resending password reset code.");
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError,
                "An error occurred while resending the password reset code.");
        }
    }
    private async Task<ServiceResult<ResultDetails>> HandleEmailCodeAsync(string email, VerificationCodeType verificationCodeType)
    {
        using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            // Fetch user by email (primary or temp)
            var user = await userRepository.GetByEmailAsync(email, verificationCodeType) ??
                       throw new NotFoundException($"User with email : {email} not found!");

            // Check if the primary email is already verified
            if (verificationCodeType == VerificationCodeType.Email && user.EmailConfirmed)
            {
                return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status400BadRequest,
                    "The email is already verified. No need to resend the code.");
            }

            // Check if the temporary email is already verified
            if (verificationCodeType == VerificationCodeType.NewEmail && user.TempEmailConfirmed)
            {
                return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status400BadRequest,
                    "The email is already verified. No need to resend the code.");
            }

            // Check if the code is still valid
            var remainingMinutes = GetRemainingMinutes(user, verificationCodeType);
            if (remainingMinutes > 0)
            {
                return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status401Unauthorized,
                    $"Retry in {remainingMinutes} minutes.");
            }

            // Prepare request and send verification code
            var verificationRequest = new VerificationRequest
            {
                User = user,
                CodeType = verificationCodeType,
                Title = "Verify your email",
                Message = "Your new verification code:"
            };

            if (await SendVerificationCodeAsync(verificationRequest))
            {
                await transaction.CommitAsync();
                logger.LogInformation("Verification code resent to email: {Email}", user.Email);
                return ServiceResult<ResultDetails>.InfoResult(StatusCodes.Status200OK,
                    "Verification code has been resent to your email.");
            }

            await transaction.RollbackAsync();
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError,
                "Failed to resend email verification code.");
        }
        catch (NotFoundException ex)
        {
            logger.LogError(ex, "User not found: {Email}", email);
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status404NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error occurred while resending email verification code.");
            return ServiceResult<ResultDetails>.ErrorResult(StatusCodes.Status500InternalServerError,
                "An error occurred while resending the email verification code.");
        }
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

            // Populate the email template and send the email
            var emailBody = await emailService.GetEmailTemplateAsync("VerificationEmailTemplate", placeholders);
            var emailTo = request.CodeType == VerificationCodeType.NewEmail ? request.User.TempEmail : request.User.Email;
            var subject = GetEmailSubject(request.CodeType);
            await emailService.SendEmailAsync(emailTo!, subject, emailBody);

            // Update user with the new verification code
            if (request.CodeType == VerificationCodeType.Password)
            {
                request.User.ResetCode = verificationCode;
                request.User.ResetCodeExpiration = codeExpiration;
            }
            else
            {
                request.User.EmailCode = verificationCode;
                request.User.EmailCodeExpiration = codeExpiration;
            }

            await userManager.UpdateAsync(request.User);

            logger.LogInformation("Sent {Context} code to {Email}.", subject, emailTo);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification email to {Email}.", request.User.Email);
            return false;
        }
    }
    private User CreateUser(RegisterDto registerDTO)
    {
        var normalizedEmail = CoreUtilities.NormalizeEmail(registerDTO.Email);
        return new User
        {
            CIN = registerDTO.CIN,
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
                CIN = registerDTO.CIN,
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
    private string GetEmailSubject(VerificationCodeType type)
    {
        return type switch
        {
            VerificationCodeType.Email or VerificationCodeType.NewEmail 
              => "Let's verify your email",
            VerificationCodeType.Password => "Let's reset your password",
            _ => "Verification Code"
        };
    }
    private double GetRemainingMinutes(User user, VerificationCodeType verificationCodeType)
    {
        var expiration = verificationCodeType == VerificationCodeType.Password
                         ? user.ResetCodeExpiration
                         : user.EmailCodeExpiration;

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