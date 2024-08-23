using RenTN.Application.DTOs.ChangeLogDTOs;
using RenTN.Application.Utilities;

namespace RenTN.Application.Services.AdminService;

public interface IAdminService
{
    Task<ApplicationResponse> GetUserByIdAsync(int id);
    Task<ApplicationResponse> GetUsersAsync(string? userRole = null);
    Task<ApplicationResponse> GetChangeLogs(DateRangeDTO dateRange);
}
