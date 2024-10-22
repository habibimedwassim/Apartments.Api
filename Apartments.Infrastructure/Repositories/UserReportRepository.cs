using Apartments.Application.Common;
using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apartments.Infrastructure.Repositories;

public class UserReportRepository(ApplicationDbContext dbContext) : 
    BaseRepository<UserReport>(dbContext), IUserReportRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    public async Task<UserReport> AddReportAsync(UserReport userReport)
    {
        return await AddAsync(userReport);
    }

    public async Task<UserReport?> GetReportByIdAsync(int id)
    {
        return await _dbContext.UserReports
                               .Include(x => x.Reporter)
                               .Include(x => x.Target)
                               .FirstOrDefaultAsync(x => x.Id == id);
    }
    public async Task<List<UserReport>> GetAdminReports()
    {
        var reports = await _dbContext.UserReports
                                      .Include(x => x.Reporter)
                                      .Include(x => x.Target)
                                      .Where(x => x.TargetRole == UserRoles.Admin)
                                      .OrderByDescending(x => x.CreatedDate)
                                      .ToListAsync();
        return reports;
    }
    public async Task<List<UserReport>> GetMyReports(string id)
    {
        var reports = await _dbContext.UserReports
                                      .Include(x => x.Reporter)
                                      .Include(x => x.Target)
                                      .Where(x => x.ReporterId == id)
                                      .OrderByDescending(x => x.CreatedDate)
                                      .ToListAsync();
        return reports;
    }
    public async Task UpdateAsync(UserReport originalRecord, UserReport updatedRecord, string userEmail,
        string[]? additionalPropertiesToExclude = null)
    {
        await UpdateWithChangeLogsAsync(originalRecord, updatedRecord, userEmail, originalRecord.Id.ToString(),
            additionalPropertiesToExclude);
    }

    public async Task DeleteAsync(UserReport report)
    {
        _dbContext.UserReports.Remove(report);
        await _dbContext.SaveChangesAsync();
    }
    public async Task<PagedModel<UserReport>> GetSentReportsPagedAsync(UserReportQueryFilter filter, string userId)
    {
        var baseQuery = _dbContext.UserReports
            .Include(x => x.Reporter)
            .Include(x => x.Target)
            .Where(x => x.ReporterId == userId);

        // Apply additional filters, sorting, and pagination
        ApplyFilters(ref baseQuery, filter);
        ApplySorting(ref baseQuery, filter);
        var totalCount = await baseQuery.CountAsync();
        var reports = await baseQuery.Skip(AppConstants.PageSize * (filter.PageNumber - 1))
                                     .Take(AppConstants.PageSize)
                                     .ToListAsync();

        return new PagedModel<UserReport> { Data = reports, DataCount = totalCount };
    }

    public async Task<PagedModel<UserReport>> GetReceivedReportsPagedAsync(UserReportQueryFilter filter, string userId, bool isAdmin)
    {
        var baseQuery = _dbContext.UserReports
            .Include(x => x.Reporter)
            .Include(x => x.Target)
            .AsQueryable();

        // If the user is an Admin, filter by TargetRole
        if (isAdmin)
        {
            baseQuery = baseQuery.Where(x => x.TargetRole == UserRoles.Admin);
        }
        else
        {
            baseQuery = baseQuery.Where(x => x.TargetId == userId && x.TargetRole == UserRoles.Owner);
        }

        // Apply additional filters, sorting, and pagination
        ApplyFilters(ref baseQuery, filter);
        ApplySorting(ref baseQuery, filter);
        var totalCount = await baseQuery.CountAsync();
        var reports = await baseQuery.Skip(AppConstants.PageSize * (filter.PageNumber - 1))
                                     .Take(AppConstants.PageSize)
                                     .ToListAsync();

        return new PagedModel<UserReport> { Data = reports, DataCount = totalCount };
    }

    private void ApplyFilters(ref IQueryable<UserReport> baseQuery, UserReportQueryFilter filter)
    {
        if (!string.IsNullOrEmpty(filter.Status))
        {
            baseQuery = baseQuery.Where(x => x.Status == filter.Status);
        }

        if (filter.FromDate.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.CreatedDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.CreatedDate <= filter.ToDate.Value);
        }
    }

    private void ApplySorting(ref IQueryable<UserReport> baseQuery, UserReportQueryFilter filter)
    {
        var sortBy = filter.SortBy ?? nameof(UserReport.CreatedDate);
        var sortDirection = filter.SortDirection;

        var columnSelector = new Dictionary<string, Expression<Func<UserReport, object>>>
        {
            { nameof(UserReport.CreatedDate), x => x.CreatedDate },
        };

        if (columnSelector.ContainsKey(sortBy))
        {
            var selectedColumn = columnSelector[sortBy];
            baseQuery = sortDirection == SortDirection.Descending
                ? baseQuery.OrderByDescending(selectedColumn)
                : baseQuery.OrderBy(selectedColumn);
        }
    }
}
