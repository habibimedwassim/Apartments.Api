using Apartments.Application.Common;
using Apartments.Application.Dtos.AuthDtos;
using Apartments.Application.Dtos.UserDtos;

namespace Apartments.Application.IServices;

public interface IAuthService
{
    Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginDto loginDTO);
    Task<ServiceResult<ResultDetails>> RegisterAsync(RegisterDto loginDTO);
    Task<ServiceResult<ResultDetails>> RegisterWithRoleAsync(RegisterDto registerDto, string role);
    Task<ServiceResult<ResultDetails>> VerifyEmailAsync(VerifyEmailDto verifyEmailDTO);
    Task<ServiceResult<ResultDetails>> ResendEmailAsync(EmailDto email, string type);
    Task<ServiceResult<ResultDetails>> ForgotPasswordAsync(EmailDto email);
    Task<ServiceResult<ResultDetails>> ResetPasswordAsync(ResetPasswordDto resetPasswordDTO);
    Task<ServiceResult<string>> UpdateUserPassword(ChangePasswordDto changePasswordDto);
    Task<ServiceResult<string>> UpdateUserEmail(UpdateEmailDto changeEmailDto);
}