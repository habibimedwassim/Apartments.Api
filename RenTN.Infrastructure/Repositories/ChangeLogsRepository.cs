using Microsoft.EntityFrameworkCore;
using RenTN.Domain.Entities;
using RenTN.Domain.Interfaces;
using RenTN.Infrastructure.Data;

namespace RenTN.Infrastructure.Repositories;

internal class ChangeLogsRepository(ApplicationDbContext _dbContext) : IChangeLogsRepository
{
    public async Task AddChangeLogs(List<ChangeLog> changeLogs)
    {
        if (changeLogs.Count > 0)
        {
            await _dbContext.ChangeLogs.AddRangeAsync(changeLogs);
        }
    }

    public async Task<IEnumerable<ChangeLog>> GetChangeLogsAsync(string entityName, DateTime startDate, DateTime endDate)
    {
        return await _dbContext.ChangeLogs
                               .Where(x => x.EntityType.ToLower() == entityName.ToLower() && 
                                           x.ChangedAt.Date >= startDate && 
                                           x.ChangedAt.Date <= endDate)
                               .ToListAsync();
    }
}
