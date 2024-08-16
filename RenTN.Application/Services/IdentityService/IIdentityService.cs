using RenTN.Application.DTOs.IdentityDTO;

namespace RenTN.Application.Services.IdentityService;

public interface IIdentityService
{
    Task AssignRole(AssignRoleDTO assignRoleDTO);
    Task UnassignRole(AssignRoleDTO unassignRoleDTO);
}
