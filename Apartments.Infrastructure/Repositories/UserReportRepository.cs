using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

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
}
