using RenTN.Application.DTOs.AuthDTOs;
using RenTN.Application.Utilities;

namespace RenTN.Application.Services.Authentication;

public interface IAuthService
{
    Task<ApplicationResponse> ResendEmailAsync(EmailDTO email);
    Task<ApplicationResponse> ForgotPasswordAsync(EmailDTO email);
    Task<ApplicationResponse> RegisterAsync(RegisterDTO registerDTO);
    Task<ApplicationResponse> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO);
    Task<ApplicationResponse> LoginAsync(LoginDTO loginDTO);
    Task<ApplicationResponse> ResetPasswordAsync(ResetPasswordDTO resetPasswordDTO);
}
