using Apartments.Application.Common;
using Apartments.Application.Dtos.AdminDtos;
using Apartments.Domain.Entities;

namespace Apartments.Application.IServices;

public interface IAdminService
{
    Task<ServiceResult<string>> AssignRole(AssignRoleDto assignRoleDto);
    Task<ServiceResult<string>> UnAssignRole(int sysId);
    Task<ServiceResult<string>> DisableUser(int id);
    Task<ServiceResult<string>> RestoreUser(int id);
    Task<ServiceResult<string>> CleanupAllOrphanedPhotosAsync(int batchSize = 100);
    Task<ServiceResult<IEnumerable<ChangeLog>>> GetChangeLogs(ChangeLogDto changeLogDto);
    Task<ServiceResult<AdminStatisticsDto>> GetStatistics(string type);
}