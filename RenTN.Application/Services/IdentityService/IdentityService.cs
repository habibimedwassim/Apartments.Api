using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Domain.Entities;
using RenTN.Domain.Exceptions;

namespace RenTN.Application.Services.IdentityService;

public class IdentityService(
    ILogger<IdentityService> _logger,
    UserManager<User> _userManager,
    RoleManager<IdentityRole> _roleManager) : IIdentityService
{
    public async Task AssignRole(AssignRoleDTO assignRoleDTO)
    {
        _logger.LogInformation("Assigning user role: {@Request}", assignRoleDTO);

        var user = await _userManager.FindByEmailAsync(assignRoleDTO.UserEmail) ??
                   throw new NotFoundException(nameof(User), assignRoleDTO.UserEmail);

        var role = await _roleManager.FindByNameAsync(assignRoleDTO.RoleName) ??
                  throw new NotFoundException(nameof(IdentityRole), assignRoleDTO.RoleName);

        await _userManager.AddToRoleAsync(user, role.Name!);
    }

    public async Task UnassignRole(AssignRoleDTO unassignRoleDTO)
    {
        _logger.LogInformation("Unassigning user role: {@Request}", unassignRoleDTO);

        var user = await _userManager.FindByEmailAsync(unassignRoleDTO.UserEmail) ??
                   throw new NotFoundException(nameof(User), unassignRoleDTO.UserEmail);

        var role = await _roleManager.FindByNameAsync(unassignRoleDTO.RoleName) ??
                  throw new NotFoundException(nameof(IdentityRole), unassignRoleDTO.RoleName);

        await _userManager.RemoveFromRoleAsync(user, role.Name!);
    }
}