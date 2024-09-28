using Apartments.Application.Common;
using Apartments.Application.Dtos.AdminDtos;
using Apartments.Application.IServices;
using Apartments.Application.Utilities;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Apartments.Application.Services;

public class AdminService(
    ILogger<AdminService> logger,
    IMapper mapper,
    IUserContext userContext,
    IAzureBlobStorageService azureBlobStorageService,
    IApartmentPhotoRepository apartmentPhotoRepository,
    IAdminRepository adminRepository,
    IUserRepository userRepository,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager
    ) : IAdminService
{
    public async Task<ServiceResult<string>> CleanupAllOrphanedPhotosAsync(int batchSize = 100)
    {
        logger.LogInformation("Admin cleanup: Cleaning up all orphaned photos with batch size {BatchSize}", batchSize);

        // Fetch all apartment photos from the database in batches
        var allApartmentPhotos = await apartmentPhotoRepository.GetPhotosInBatchesAsync(batchSize);
        var dbPhotoUrls = allApartmentPhotos.Select(p => p.Url).ToHashSet();

        // Check for orphaned photos (those in the database but missing in Azure Blob Storage)
        var orphanedPhotoUrls = await azureBlobStorageService.FindMissingPhotosInAzureAsync(dbPhotoUrls, batchSize);

        if (!orphanedPhotoUrls.Any())
        {
            logger.LogInformation("No orphaned photos found.");
            return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, "No orphaned photos found.");
        }

        // Delete orphaned photos from the database
        var orphanedPhotosToDelete = allApartmentPhotos.Where(x => orphanedPhotoUrls.Contains(x.Url)).ToList();
        await apartmentPhotoRepository.PermanentDeleteApartmentPhotosAsync(orphanedPhotosToDelete);

        var logMessage = $"{orphanedPhotosToDelete.Count} orphaned photos deleted successfully.";
        logger.LogInformation(logMessage);
        return ServiceResult<string>.InfoResult(StatusCodes.Status200OK, logMessage);
    }
    

    public async Task<ServiceResult<IEnumerable<ChangeLog>>> GetChangeLogs(ChangeLogDto changeLogDto)
    {
        var endDate = changeLogDto.EndDate ?? DateTime.UtcNow;
        logger.LogInformation("Retrieving change logs for entity {EntityName} between {StartDate} and {EndDate}.",
                              changeLogDto.EntityName, changeLogDto.StartDate, endDate);

        var changeLogs = await adminRepository.GetChangeLogsAsync(changeLogDto.EntityName, changeLogDto.StartDate, endDate);

        return ServiceResult<IEnumerable<ChangeLog>>.SuccessResult(changeLogs);

    }
    public async Task<ServiceResult<AdminStatisticsDto>> GetStatistics(string type)
    {
        var parsedType = CoreUtilities.ValidateEnum<StatisticsType>(type);

        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Retrieving statistics for {type}", parsedType.ToString());

        var (active, deleted) = await adminRepository.GetStatisticsForTypeAsync(parsedType);

        var statisticsResult = new AdminStatisticsDto() 
        {
            Active = active,
            Deleted = deleted
        };

        return ServiceResult<AdminStatisticsDto>.SuccessResult(statisticsResult);
    }

    public async Task<ServiceResult<string>> AssignRole(AssignRoleDto assignRoleDto)
    {
        logger.LogInformation("Assigning user role: {@Request}", assignRoleDto);

        var user = await userRepository.GetBySysIdAsync(assignRoleDto.UserId) ??
                   throw new NotFoundException("User not found");

        var roles = await userManager.GetRolesAsync(user);
        if (roles.Any())
        {
            var removeRolesResult = await userManager.RemoveFromRolesAsync(user, roles);
            if (!removeRolesResult.Succeeded)
            {
                return ServiceResult<string>.ErrorResult(StatusCodes.Status417ExpectationFailed, "Failed to remove existing roles");
            }
        }

        var role = await roleManager.FindByNameAsync(assignRoleDto.RoleName) ??
                   throw new NotFoundException(nameof(IdentityRole), assignRoleDto.RoleName);

        var addRoleResult = await userManager.AddToRoleAsync(user, role.Name!);
        if (!addRoleResult.Succeeded)
        {
            return ServiceResult<string>.ErrorResult(StatusCodes.Status417ExpectationFailed, "Failed to assign new role");
        }

        return await AddRoleToUser(user, role.Name, "assign role");
    }
    public async Task<ServiceResult<string>> UnAssignRole(int sysId)
    {
        logger.LogInformation("Unassigning user roles");

        var user = await userRepository.GetBySysIdAsync(sysId) ??
                   throw new NotFoundException("User not found");

        var rolesRemoved = await RemoveUserRoles(user);
        if (!rolesRemoved)
        {
            return ServiceResult<string>.ErrorResult(StatusCodes.Status417ExpectationFailed, "Failed to remove existing roles");
        }

        return await AddRoleToUser(user, null, "unassign role");
    }
    public async Task<ServiceResult<string>> DisableUser(int id)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Disabling user with Id = {Id}", id);

        var user = await userRepository.GetBySysIdAsync(id) ??
                   throw new NotFoundException("User not found");

        await userRepository.SoftDeleteUserAsync(user, currentUser.Email);

        return ServiceResult<string>.InfoResult(StatusCodes.Status202Accepted, "User disabled successfully.");
    }
    public async Task<ServiceResult<string>> RestoreUser(int id)
    {
        var currentUser = userContext.GetCurrentUser();

        logger.LogInformation("Restoring user with Id = {Id}", id);

        var user = await userRepository.GetBySysIdAsync(id) ??
                   throw new NotFoundException("User not found");

        await userRepository.RestoreUserAsync(user, currentUser.Email);

        return ServiceResult<string>.InfoResult(StatusCodes.Status202Accepted, "User restored successfully.");
    }

    private async Task<bool> RemoveUserRoles(User user)
    {
        try
        {
            var roles = await userManager.GetRolesAsync(user);
            if (roles.Any())
            {
                var removeRolesResult = await userManager.RemoveFromRolesAsync(user, roles);
                return removeRolesResult.Succeeded;
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return false;
        }
    }
    private async Task<ServiceResult<string>> AddRoleToUser(User user, string? role, string message)
    {
        try
        {
            var currentUser = userContext.GetCurrentUser();
            var originalUser = mapper.Map<User>(user);
            user.Role = role;
            await userRepository.UpdateAsync(originalUser, user, currentUser.Email);

            return ServiceResult<string>.InfoResult(StatusCodes.Status202Accepted, $"{message} succeeded!");
        }
        catch (Exception ex) 
        {
            logger.LogError(ex, ex.Message);
            return ServiceResult<string>.ErrorResult(StatusCodes.Status500InternalServerError, $"{message} failed");
        }
        
    }
}
