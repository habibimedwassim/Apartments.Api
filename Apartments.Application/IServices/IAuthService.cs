using Apartments.Application.Common;
using Apartments.Application.Dtos.AuthDtos;
using Apartments.Application.Dtos.UserDtos;

namespace Apartments.Application.IServices;

public interface IAuthService
{
    Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginDto loginDTO);
    Task<ServiceResult<ResultDetails>> RegisterAsync(RegisterDto loginDTO, string? role = null);
    Task<ServiceResult<ResultDetails>> VerifyEmailAsync(VerifyEmailDto verifyEmailDTO);
    Task<ServiceResult<ResultDetails>> ResendEmailAsync(EmailDto email);
    Task<ServiceResult<ResultDetails>> ForgotPasswordAsync(EmailDto email);
    Task<ServiceResult<ResultDetails>> ResetPasswordAsync(ResetPasswordDto resetPasswordDTO);
    Task<ServiceResult<string>> UpdateUserPassword(ChangePasswordDto changePasswordDto);
    Task<ServiceResult<string>> UpdateUserEmail(EmailDto changeEmailDto);
}