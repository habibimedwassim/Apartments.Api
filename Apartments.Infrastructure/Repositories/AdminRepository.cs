using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apartments.Infrastructure.Repositories;

public class AdminRepository(ApplicationDbContext dbContext) : IAdminRepository
{
    public async Task<PagedModel<ChangeLog>> GetChangeLogsPagedAsync(
    string entityName, DateTime startDate, DateTime endDate, ChangeLogQueryFilter filter)
    {
        var baseQuery = dbContext.ChangeLogs.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(entityName))
        {
            baseQuery = baseQuery.Where(x => x.EntityType.ToLower() == entityName.ToLower());
        }

        baseQuery = baseQuery.Where(x => x.ChangedAt >= startDate && x.ChangedAt <= endDate);

        // Get the total count before pagination
        var totalCount = await baseQuery.CountAsync();

        // Apply sorting
        ApplySorting(ref baseQuery, filter);

        // Apply pagination
        var changeLogs = await baseQuery
            .Skip(AppConstants.PageSize * (filter.PageNumber - 1))
            .Take(AppConstants.PageSize)
            .ToListAsync();

        return new PagedModel<ChangeLog> { Data = changeLogs, DataCount = totalCount };
    }


    private void ApplySorting(ref IQueryable<ChangeLog> baseQuery, ChangeLogQueryFilter filter)
    {
        var sortBy = filter.SortBy ?? nameof(ChangeLog.ChangedAt);
        var sortDirection = filter.SortDirection;

        // Define column selector based on the sortBy value
        var columnSelector = new Dictionary<string, Expression<Func<ChangeLog, object>>>
        {
            { nameof(ChangeLog.ChangedAt), x => x.ChangedAt },
            { nameof(ChangeLog.PropertyName), x => x.PropertyName },
            { nameof(ChangeLog.EntityType), x => x.EntityType }
        };

        if (columnSelector.ContainsKey(sortBy))
        {
            var selectedColumn = columnSelector[sortBy];
            baseQuery = sortDirection == SortDirection.Descending
                ? baseQuery.OrderByDescending(selectedColumn)
                : baseQuery.OrderBy(selectedColumn);
        }
    }


    public async Task<IEnumerable<ChangeLog>> GetChangeLogsAsync(string entityName, DateTime startDate,
        DateTime endDate)
    {
        return await dbContext.ChangeLogs
            .Where(x => x.EntityType.ToLower() == entityName.ToLower() &&
                        x.ChangedAt.Date >= startDate &&
                        x.ChangedAt.Date <= endDate)
            .ToListAsync();
    }

    public async Task<(int active, int deleted)> GetStatisticsForTypeAsync(StatisticsType statisticsType)
    {
        return statisticsType switch
        {
            StatisticsType.Users => await GetActiveAndDeletedCountsAsync(dbContext.Users),
            StatisticsType.Apartments => await GetActiveAndDeletedCountsAsync(dbContext.Apartments),
            StatisticsType.Photos => await GetActiveAndDeletedCountsAsync(dbContext.ApartmentPhotos),
            StatisticsType.RentRequests => await GetActiveAndDeletedCountsAsync(dbContext.ApartmentRequests,
                ApartmentRequestType.Rent.ToString()),
            StatisticsType.DismissRequests => await GetActiveAndDeletedCountsAsync(dbContext.ApartmentRequests,
                ApartmentRequestType.Dismiss.ToString()),
            StatisticsType.LeaveRequests => await GetActiveAndDeletedCountsAsync(dbContext.ApartmentRequests,
                ApartmentRequestType.Leave.ToString()),
            _ => (0, 0)
        };
    }

    private async Task<(int active, int deleted)> GetActiveAndDeletedCountsAsync<T>(IQueryable<T> query,
        string? type = null) where T : class
    {
        var countQuery = query.IgnoreQueryFilters();
        if (!string.IsNullOrEmpty(type))
            countQuery = countQuery.Where(x => EF.Property<string>(x, "RequestType").ToLower() == type.ToLower());
        var activeCount = await countQuery.CountAsync(x => EF.Property<bool>(x, "IsDeleted") == false);
        var deletedCount = await countQuery.CountAsync(x => EF.Property<bool>(x, "IsDeleted") == true);
        return (activeCount, deletedCount);
    }
}