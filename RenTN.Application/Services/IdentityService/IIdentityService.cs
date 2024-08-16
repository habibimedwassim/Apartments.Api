using Microsoft.AspNetCore.Identity;
using RenTN.Application.DTOs.IdentityDTO;

namespace RenTN.Application.Services.IdentityService;

public interface IIdentityService
{
    Task<(bool Success, AuthResponse? Response, string Message)> LoginAsync(LoginDTO loginDTO);
    Task<(bool Success, string Message)> RegisterAsync(RegisterDTO registerDTO);
    Task<(bool Success, string Message)> VerifyEmailAsync(VerifyEmailDTO verifyEmailDTO);
    Task AssignRole(AssignRoleDTO assignRoleDTO);
    Task UnassignRole(AssignRoleDTO unassignRoleDTO);
}
