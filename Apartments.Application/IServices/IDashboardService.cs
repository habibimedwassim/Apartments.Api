using Apartments.Application.Common;
using Apartments.Application.Dtos.DashboardDtos;
using Apartments.Application.Services;

namespace Apartments.Application.IServices;

public interface IDashboardService
{
    Task<ServiceResult<AdminDashboardDto>> GetAdminDashboard();
    Task<ServiceResult<OwnerDashboardDto>> GetOwnerDashboard();
}
