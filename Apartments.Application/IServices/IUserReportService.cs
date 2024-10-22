using Apartments.Application.Common;
using Apartments.Application.Dtos.UserReportDtos;
using Apartments.Domain.Common;
using Apartments.Domain.QueryFilters;

namespace Apartments.Application.IServices;
public interface IUserReportService
{
    Task<ServiceResult<string>> CreateUserReport(CreateUserReportDto createUserReportDto);
    //Task<ServiceResult<List<UserReportDto>>> GetUserReports();
    Task<ServiceResult<string>> UpdateUserReport(int id, UpdateUserReportDto updateReportDto);
    Task<ServiceResult<string>> DeleteUserReport(int id);
    Task<ServiceResult<PagedResult<UserReportDto>>> GetUserReportsPaged(UserReportQueryFilter filter);
    Task<ServiceResult<UserReportDto>> GetReportById(int id);
}