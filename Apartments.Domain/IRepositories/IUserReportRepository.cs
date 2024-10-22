using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.QueryFilters;

namespace Apartments.Domain.IRepositories;
public interface IUserReportRepository
{
    Task<UserReport> AddReportAsync(UserReport userReport);
    Task DeleteAsync(UserReport report);
    Task<List<UserReport>> GetAdminReports();
    Task<List<UserReport>> GetMyReports(string id);
    Task<PagedModel<UserReport>> GetReceivedReportsPagedAsync(UserReportQueryFilter filter, string userId, bool isAdmin);
    Task<UserReport?> GetReportByIdAsync(int id);
    Task<PagedModel<UserReport>> GetSentReportsPagedAsync(UserReportQueryFilter filter, string userId);
    Task UpdateAsync(UserReport originalRecord, UserReport updatedRecord, string userEmail, string[]? additionalPropertiesToExclude = null);
}