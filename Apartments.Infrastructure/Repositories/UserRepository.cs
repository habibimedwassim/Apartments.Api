using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Apartments.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext dbContext) : BaseRepository<User>(dbContext), IUserRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<User?> GetByUserIdAsync(string id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<User?> GetBySysIdAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<User?> GetTenantByApartmentId(int id)
    {
        var apartment = await _dbContext.Apartments.Include(x => x.Tenant).FirstOrDefaultAsync(x => x.Id == id);

        return apartment?.Tenant;
    }

    public async Task UpdateAsync(User originalRecord, User updatedRecord, string userEmail,
        string[]? additionalPropertiesToExclude = null)
    {
        await UpdateWithChangeLogsAsync(originalRecord, updatedRecord, userEmail, originalRecord.SysId.ToString(),
            additionalPropertiesToExclude);
    }

    public async Task SoftDeleteUserAsync(User user, string userEmail)
    {
        if (user.IsDeleted) return;

        await DeleteRestoreAsync(user, true, userEmail, user.SysId.ToString());
    }

    public async Task RestoreUserAsync(User user, string userEmail)
    {
        if (!user.IsDeleted) return;

        await DeleteRestoreAsync(user, false, userEmail, user.SysId.ToString());
    }
}