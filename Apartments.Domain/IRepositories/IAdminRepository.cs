using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.QueryFilters;

namespace Apartments.Domain.IRepositories;

public interface IAdminRepository
{
    Task<IEnumerable<ChangeLog>> GetChangeLogsAsync(string entityName, DateTime startDate, DateTime endDate);
    Task<PagedModel<ChangeLog>> GetChangeLogsPagedAsync(string entityName, DateTime startDate, DateTime endDate, ChangeLogQueryFilter filter);
    Task<(int active, int deleted)> GetStatisticsForTypeAsync(StatisticsType statisticsType);
}