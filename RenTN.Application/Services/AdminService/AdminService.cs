using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Application.DTOs.IdentityDTOs;
using RenTN.Domain.Common;
using RenTN.Domain.Entities;

namespace RenTN.Application.Services.AdminService;

public class AdminService(
    ILogger<AdminService> _logger,
    IMapper _mapper,
    UserManager<User> _userManager) : IAdminService
{
    public async Task<IEnumerable<object>> GetUsersAsync(string? userRole = null)
    {
        _logger.LogInformation("Retrieving {UserRole}s", userRole ?? "Users");

        List<User> users;

        if (userRole == UserRoles.Admin)
        {
            users = (await _userManager.GetUsersInRoleAsync(UserRoles.Admin)).ToList();
            return _mapper.Map<List<AdminProfileDTO>>(users);
        }
        else if (userRole == UserRoles.Owner)
        {
            users = (await _userManager.GetUsersInRoleAsync(UserRoles.Owner)).ToList();
            return _mapper.Map<List<OwnerProfileDTO>>(users);
        }
        else
        {
            var admins = await _userManager.GetUsersInRoleAsync(UserRoles.Admin);
            var owners = await _userManager.GetUsersInRoleAsync(UserRoles.Owner);

            users = await _userManager.Users
                .Where(user => !user.IsDeleted)
                .ToListAsync();

            var filteredUsers = users
                .Except(admins.Concat(owners))
                .ToList();

            return _mapper.Map<List<UserProfileDTO>>(filteredUsers);
        }
    }
}
