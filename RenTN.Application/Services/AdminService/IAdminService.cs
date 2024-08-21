using RenTN.Application.DTOs.ChangeLogDTOs;
using RenTN.Application.DTOs.IdentityDTO;
using RenTN.Domain.Entities;

namespace RenTN.Application.Services.AdminService;

public interface IAdminService
{
    Task<object> GetUserByIdAsync(int id);
    Task<IEnumerable<object>> GetUsersAsync(string? userRole = null);
    Task<IEnumerable<ChangeLog>> GetChangeLogs(DateRangeDTO dateRange);
}
