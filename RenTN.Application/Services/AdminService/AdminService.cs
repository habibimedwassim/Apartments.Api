using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Application.DTOs.ChangeLogDTOs;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Application.DTOs.IdentityDTOs;
using RenTN.Domain.Common;
using RenTN.Domain.Entities;
using RenTN.Domain.Exceptions;
using RenTN.Domain.Interfaces;

namespace RenTN.Application.Services.AdminService;

public class AdminService(
    ILogger<AdminService> _logger,
    IMapper _mapper,
    UserManager<User> _userManager,
    IApartmentsRepository _apartmentsRepository,
    IChangeLogsRepository _changeLogsRepository) : IAdminService
{
    public Task<IEnumerable<ChangeLog>> GetChangeLogs(DateRangeDTO dateRange)
    {
        var endDate = dateRange.EndDate ?? DateTime.UtcNow;

        _logger.LogInformation("Retrieving change logs for entity {EntityName} between {StartDate} and {EndDate}.",
                              dateRange.EntityName, dateRange.StartDate, endDate);

        var changeLogs = _changeLogsRepository.GetChangeLogsAsync(dateRange.EntityName, dateRange.StartDate, endDate);

        return changeLogs;
    }

    public async Task<object> GetUserByIdAsync(int id)
    {
        var user = await _userManager.Users
                                     .Include(x => x.CurrentApartment)
                                     .FirstOrDefaultAsync(x => x.SysID == id)
                                     ?? throw new NotFoundException(nameof(User), id.ToString());

        var userRole = await _userManager.GetRolesAsync(user);

        if (userRole.Contains(UserRoles.Admin))
        {
            return _mapper.Map<AdminProfileDTO>(user);
        }
        else if(userRole.Contains(UserRoles.Owner))
        {
            var ownedApartments = await _apartmentsRepository.GetApartmentsByOwnerIdAsync(user.Id);
            var ownedApartmentDTO = _mapper.Map<List<ApartmentDTO>>(ownedApartments);
            var userRecord = _mapper.Map<OwnerProfileDTO>(user);
            userRecord.OwnedApartments.AddRange(ownedApartmentDTO);
            return userRecord;
        }
        else
        {
            return _mapper.Map<UserProfileDTO>(user);
        }

    }

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
                .Include(x => x.CurrentApartment)
                .Where(user => !user.IsDeleted)
                .ToListAsync();

            var filteredUsers = users
                .Except(admins.Concat(owners))
                .ToList();

            return _mapper.Map<List<UserProfileDTO>>(filteredUsers);
        }
    }
}
