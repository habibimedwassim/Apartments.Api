using Apartments.Domain.Entities;

namespace Apartments.Domain.IRepositories;
public interface IUserReportRepository
{
    Task<UserReport> AddReportAsync(UserReport userReport);
    Task DeleteAsync(UserReport report);
    Task<List<UserReport>> GetAdminReports();
    Task<List<UserReport>> GetMyReports(string id);
    Task<UserReport?> GetReportByIdAsync(int id);
    Task UpdateAsync(UserReport originalRecord, UserReport updatedRecord, string userEmail, string[]? additionalPropertiesToExclude = null);
}