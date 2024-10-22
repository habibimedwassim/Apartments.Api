using Apartments.Application.Common;
using Apartments.Application.Dtos.UserReportDtos;

namespace Apartments.Application.IServices;
public interface IUserReportService
{
    Task<ServiceResult<string>> CreateUserReport(CreateUserReportDto createUserReportDto);
    Task<ServiceResult<List<UserReportDto>>> GetUserReports();
    Task<ServiceResult<string>> UpdateUserReport(int id, UpdateUserReportDto updateReportDto);
    Task<ServiceResult<string>> DeleteUserReport(int id);
}