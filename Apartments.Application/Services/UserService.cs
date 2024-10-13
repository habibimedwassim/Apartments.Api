using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Application.Dtos.AuthDtos;
using Apartments.Application.Dtos.EmailDtos;
using Apartments.Application.Dtos.UserDtos;
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

namespace Apartments.Application.Services;

public class UserService(
    ILogger<UserService> logger,
    IMapper mapper,
    IUserContext userContext,
    IUserRepository userRepository,
    IEmailService emailService,
    UserManager<User> userManager,
    IApartmentRepository apartmentRepository)
    : IUserService
{
    public async Task<ServiceResult<UserDto>> GetUserProfile()
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Retrieving User {userId} profile", currentUser.Email);

        var user = await userRepository.GetByUserIdAsync(currentUser.Id) ??
                   throw new NotFoundException("User not found");

        var apartment = await apartmentRepository.GetApartmentByTenantId(currentUser.Id);

        var userDto = mapper.Map<UserDto>(user);

        if(apartment != null)
        {
            var apartmentDto = mapper.Map<ApartmentDto>(apartment);
            userDto.CurrentApartment = apartmentDto;
        }

        return ServiceResult<UserDto>.SuccessResult(userDto);
    }

    public async Task<ServiceResult<string>> UpdateUserDetails(UpdateUserDto updateAppUserDto)
    {
        var currentUser = userContext.GetCurrentUser();

        var user = await userManager.FindByIdAsync(currentUser.Id) ??
                   throw new NotFoundException("User not found");

        mapper.Map(updateAppUserDto, user);
        await userManager.UpdateAsync(user);

        logger.LogInformation("User details updated successfully for {UserId}.", user.Id);
        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "User updated successfully.");
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
    public async Task<ServiceResult<string>> UpdateUserEmail(EmailDto emailDto)
    {
        using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            var currentUser = userContext.GetCurrentUser();
            var user = await userManager.FindByIdAsync(currentUser.Id) ??
                       throw new NotFoundException($"User ({currentUser.Email}) not found!");

            var normalizedEmail = CoreUtilities.NormalizeEmail(emailDto.Email);

            // Ensure no other user has the new email
            if (await userManager.Users.AnyAsync(x => x.Email == normalizedEmail || x.TempEmail == normalizedEmail))
            {
                await transaction.RollbackAsync();
                return ServiceResult<string>.ErrorResult(StatusCodes.Status409Conflict, $"Email ({emailDto.Email}) already exists.");
            }

            user.TempEmail = normalizedEmail;
            user.TempEmailConfirmed = false;

            // Send verification code to the new email
            var verificationRequest = new VerificationRequest
            {
                User = user,
                CodeType = VerificationCodeType.Email,
                Title = "Verify your new email",
                Message = "Please verify your new email address using the code below:"
            };

            // If the email was sent successfully, update the user with temporary email
            if (await SendEmailVerificationCodeAsync(verificationRequest))
            {
                user.EmailCode = verificationRequest.User.EmailCode;
                user.EmailCodeExpiration = verificationRequest.User.EmailCodeExpiration;

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
    public async Task<ServiceResult<string>> VerifyEmailAsync(VerifyNewEmailDto verifyEmailDTO)
    {
        using var transaction = await userRepository.BeginTransactionAsync();
        try
        {
            var normalizedEmail = CoreUtilities.NormalizeEmail(verifyEmailDTO.Email);
            var user = await userRepository.GetByEmailAsync(normalizedEmail, VerificationCodeType.NewEmail) ??
                       throw new NotFoundException($"User with email : {verifyEmailDTO.Email} not found!");

            // Validate password
            if (!await userManager.CheckPasswordAsync(user, verifyEmailDTO.Password))
            {
                await transaction.RollbackAsync();
                return ServiceResult<string>.ErrorResult(StatusCodes.Status400BadRequest, "Invalid current password.");
            }

            // Check if the email is already confirmed
            if (user.TempEmailConfirmed)
            {
                await transaction.RollbackAsync();
                return ServiceResult<string>.ErrorResult(StatusCodes.Status401Unauthorized,
                    $"Temporary email ({user.TempEmail}) is already verified!");
            }

            // Validate the verification code and expiration
            if (user.EmailCode != verifyEmailDTO.VerificationCode ||
                user.EmailCodeExpiration < DateTime.UtcNow)
            {
                await transaction.RollbackAsync();
                return ServiceResult<string>.ErrorResult(StatusCodes.Status401Unauthorized,
                    "Invalid or expired verification code.");
            }

            var oldEmail = user.Email!;

            // Update email details
            user.Email = normalizedEmail;
            user.UserName = normalizedEmail;
            user.NormalizedEmail = normalizedEmail.ToUpper();
            user.NormalizedUserName = normalizedEmail.ToUpper();
            user.TempEmail = null;
            user.TempEmailConfirmed = false;
            user.EmailConfirmed = true;
            user.EmailCode = null;
            user.EmailCodeExpiration = null;

            // Send notification email to old email
            var notificationRequest = new VerificationRequest
            {
                User = user,
                CodeType = VerificationCodeType.NewEmail,
                Title = "Email Updated",
                Message = "Your email has been successfully updated."
            };

            if (await SendEmailNotificationAsync(notificationRequest, oldEmail))
            {
                await userManager.UpdateAsync(user);
                await transaction.CommitAsync();
                return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "Email verified successfully.");
            }

            await transaction.RollbackAsync();
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, "Failed to verify the new email.");
        }
        catch (NotFoundException ex)
        {
            logger.LogError(ex, "User not found: {Email}", verifyEmailDTO.Email);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status404NotFound, ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Error occurred during email verification.");
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError,
                "An error occurred while verifying the email.");
        }
    }

    #region Helpers
    private async Task<bool> SendEmailVerificationCodeAsync(VerificationRequest request)
    {
        try
        {
            // Generate verification code and expiration
            request.User.EmailCode = GenerateVerificationCode();
            request.User.EmailCodeExpiration = DateTime.UtcNow.AddMinutes(AppConstants.CodeExpiration);

            // Prepare placeholders for the email sent to the new email
            var placeholders = new Dictionary<string, string>
            {
                { "Title", "Verify your new email" },
                { "UserName", request.User.FirstName ?? "" },
                { "Message", request.Message },
                { "VerificationCode", request.User.EmailCode },
                { "ExpirationTime", AppConstants.CodeExpiration.ToString() }
            };

            // Send the verification email to the new email
            var emailBody = await emailService.GetEmailTemplateAsync("VerificationEmailTemplate", placeholders);
            var subject = "Verify your email change";
            await emailService.SendEmailAsync(request.User.TempEmail!, subject, emailBody);

            logger.LogInformation("Verification email sent to: {TempEmail}", request.User.TempEmail);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification email during email change.");
            return false;
        }
    }
    private async Task<bool> SendEmailNotificationAsync(VerificationRequest request, string oldEmail)
    {
        try
        {
            // Prepare placeholders for the notification email
            var placeholders = new Dictionary<string, string>
            {
                { "Title", "Email Change Confirmation" },
                { "UserName", request.User.FirstName ?? "" },
                { "NotificationDetails", $"Your email has been successfully changed to {request.User.Email}. " +
                             "<br>If this wasn't you, please contact support immediately." }
            };

            // Send notification to the old email
            var emailBody = await emailService.GetEmailTemplateAsync("EmailNotificationTemplate", placeholders);
            var subject = "Email Change Notification";
            await emailService.SendEmailAsync(oldEmail, subject, emailBody);

            logger.LogInformation("Notification email sent to old email: {OldEmail}.", oldEmail);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send notification email during email change.");
            return false;
        }
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
    #endregion
}