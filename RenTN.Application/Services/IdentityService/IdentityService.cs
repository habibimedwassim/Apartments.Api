using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Application.Users;
using RenTN.Domain.Common;
using RenTN.Domain.Entities;
using RenTN.Domain.Exceptions;
using RenTN.Domain.Interfaces;

namespace RenTN.Application.Services.IdentityService;

public class IdentityService(
    ILogger<IdentityService> _logger,
    IMapper _mapper,
    IApartmentsRepository _apartmentsRepository,
    UserManager<User> _userManager,
    RoleManager<IdentityRole> _roleManager,
    IUserContext _userContext) : IIdentityService
{
    
    public async Task AssignRole(AssignRoleDTO assignRoleDTO)
    {
        _logger.LogInformation("Assigning user role: {@Request}", assignRoleDTO);

        var normalizedEmail = EmailNormalizer.NormalizeEmail(assignRoleDTO.UserEmail);
        var user = await _userManager.FindByEmailAsync(normalizedEmail) ??
                   throw new NotFoundException(nameof(User), assignRoleDTO.UserEmail);

        var role = await _roleManager.FindByNameAsync(assignRoleDTO.RoleName) ??
                   throw new NotFoundException(nameof(IdentityRole), assignRoleDTO.RoleName);

        await _userManager.AddToRoleAsync(user, role.Name!);
    }

    public async Task<object?> GetCurrentUserProfile()
    {
        var currentUser = _userContext.GetCurrentUser();
        if (currentUser == null)
        {
            _logger.LogWarning("User not found!");
            return null;
        }

        var user = await _userManager.FindByEmailAsync(currentUser.Email);

        if (user == null) 
        {
            _logger.LogWarning("User not found!");
            return null;
        }

        var roles = currentUser.Roles;

        if (roles.Contains("Owner"))
        {
            var ownerProfile = _mapper.Map<OwnerProfileDTO>(user);
            var ownedApartments = await _apartmentsRepository.GetApartmentsByOwnerIdAsync(user.Id);
            ownerProfile.OwnedApartments = _mapper.Map<List<ApartmentDTO>>(ownedApartments);
            return ownerProfile;
        }

        var userProfile = _mapper.Map<UserProfileDTO>(user);
        return userProfile;
    }

    public async Task UnassignRole(AssignRoleDTO unassignRoleDTO)
    {
        _logger.LogInformation("Unassigning user role: {@Request}", unassignRoleDTO);

        var normalizedEmail = EmailNormalizer.NormalizeEmail(unassignRoleDTO.UserEmail);
        var user = await _userManager.FindByEmailAsync(normalizedEmail) ??
                   throw new NotFoundException(nameof(User), unassignRoleDTO.UserEmail);

        var role = await _roleManager.FindByNameAsync(unassignRoleDTO.RoleName) ??
                   throw new NotFoundException(nameof(IdentityRole), unassignRoleDTO.RoleName);

        await _userManager.RemoveFromRoleAsync(user, role.Name!);
    }
}
