using Microsoft.AspNetCore.Identity;
using RenTN.Application.DTOs.IdentityDTO;

namespace RenTN.Application.Services.IdentityService;

public interface IIdentityService
{
    Task AssignRole(AssignRoleDTO assignRoleDTO);
    Task<object?> GetCurrentUserProfile();
    Task UnassignRole(AssignRoleDTO unassignRoleDTO);
}
