using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Application.DTOs.ChangeLogDTOs;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Application.DTOs.IdentityDTOs;
using RenTN.Application.Utilities;
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
    public async Task<ApplicationResponse> GetChangeLogs(DateRangeDTO dateRange)
    {
        var endDate = dateRange.EndDate ?? DateTime.UtcNow;

        _logger.LogInformation("Retrieving change logs for entity {EntityName} between {StartDate} and {EndDate}.",
                              dateRange.EntityName, dateRange.StartDate, endDate);

        var changeLogs = await _changeLogsRepository.GetChangeLogsAsync(dateRange.EntityName, dateRange.StartDate, endDate);

        return new ApplicationResponse(true, StatusCodes.Status200OK, $"{changeLogs.Count()} records found", changeLogs);
    }

    public async Task<ApplicationResponse> GetUserByIdAsync(int id)
    {
        var user = await _userManager.Users
                                     .Include(x => x.CurrentApartment)
                                     .FirstOrDefaultAsync(x => x.SysID == id)
                                     ?? throw new NotFoundException(nameof(User), id.ToString());

        var userRole = user.Role;

        switch (userRole)
        {
            case UserRoles.Admin:
                return new ApplicationResponse(true, StatusCodes.Status200OK, "OK", _mapper.Map<BaseProfileDTO>(user));
            case UserRoles.Owner:
                var ownedApartments = await _apartmentsRepository.GetApartmentsByOwnerIdAsync(user.Id);
                var ownedApartmentDTO = _mapper.Map<List<ApartmentDTO>>(ownedApartments);
                var userRecord = _mapper.Map<OwnerProfileDTO>(user);
                userRecord.OwnedApartments.AddRange(ownedApartmentDTO);
                return new ApplicationResponse(true, StatusCodes.Status200OK, "OK", userRecord);
            default:
                return new ApplicationResponse(true, StatusCodes.Status200OK, "OK", _mapper.Map<UserProfileDTO>(user));
        }
    }

    public async Task<ApplicationResponse> GetUsersAsync(string? userRole = null)
    {
        _logger.LogInformation("Retrieving {UserRole}s", userRole ?? "Users");

        var baseQuery = _userManager.Users
                                    .Include(x => x.CurrentApartment)
                                    .AsQueryable();

        if (userRole == null)
        {
            baseQuery = baseQuery.Where(x => x.Role != UserRoles.Admin && x.Role != UserRoles.Owner);
        }
        else
        {
            baseQuery = baseQuery.Where(x => x.Role == userRole);
        }

        var users = await baseQuery.ToListAsync();

        return userRole switch
        {
            UserRoles.Admin => new ApplicationResponse(true, StatusCodes.Status200OK, $"{users.Count} records found", _mapper.Map<List<BaseProfileDTO>>(users)),
            UserRoles.Owner => new ApplicationResponse(true, StatusCodes.Status200OK, $"{users.Count} records found", _mapper.Map<List<OwnerProfileDTO>>(users)),
            _ => new ApplicationResponse(true, StatusCodes.Status200OK, $"{users.Count} records found", _mapper.Map<List<UserProfileDTO>>(users)),
        };
    }
}