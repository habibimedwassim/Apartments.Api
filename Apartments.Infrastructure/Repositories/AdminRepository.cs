using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Apartments.Infrastructure.Repositories;

public class AdminRepository(ApplicationDbContext dbContext) : IAdminRepository
{
    public async Task<IEnumerable<ChangeLog>> GetChangeLogsAsync(string entityName, DateTime startDate, DateTime endDate)
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
            StatisticsType.RentRequests => await GetActiveAndDeletedCountsAsync(dbContext.ApartmentRequests, ApartmentRequestType.Rent.ToString()),
            StatisticsType.DismissRequests => await GetActiveAndDeletedCountsAsync(dbContext.ApartmentRequests, ApartmentRequestType.Dismiss.ToString()),
            StatisticsType.LeaveRequests => await GetActiveAndDeletedCountsAsync(dbContext.ApartmentRequests, ApartmentRequestType.Leave.ToString()),
            _ => (0, 0),
        };
    }

    private async Task<(int active, int deleted)> GetActiveAndDeletedCountsAsync<T>(IQueryable<T> query, string? type = null) where T : class
    {
        var countQuery = query.IgnoreQueryFilters();
        if (!string.IsNullOrEmpty(type)) 
        {
            countQuery = countQuery.Where(x => EF.Property<string>(x, "RequestType").ToLower() == type.ToLower());
        }
        var activeCount = await countQuery.CountAsync(x => EF.Property<bool>(x, "IsDeleted") == false);
        var deletedCount = await countQuery.CountAsync(x => EF.Property<bool>(x, "IsDeleted") == true);
        return (activeCount, deletedCount);
    }
}
