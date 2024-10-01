using Apartments.Domain.Common;
using Apartments.Domain.Entities;

namespace Apartments.Domain.IRepositories;

public interface IAdminRepository
{
    Task<IEnumerable<ChangeLog>> GetChangeLogsAsync(string entityName, DateTime startDate, DateTime endDate);
    Task<(int active, int deleted)> GetStatisticsForTypeAsync(StatisticsType statisticsType);
}