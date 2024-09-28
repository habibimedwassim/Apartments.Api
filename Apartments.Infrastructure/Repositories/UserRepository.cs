using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Apartments.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly ApplicationDbContext dbContext;
    public UserRepository(ApplicationDbContext _dbContext) : base(_dbContext)
    {
        dbContext = _dbContext;
    }

    public async Task<User?> GetByUserIdAsync(string id)
        => await GetByIdAsync(id);
    public async Task<User?> GetBySysIdAsync(int id)
        => await GetByIdAsync(id);
    public async Task<User?> GetTenantByApartmentId(int id)
        => await dbContext.Users.FirstOrDefaultAsync(x => x.CurrentApartmentId == id);
    public async Task UpdateAsync(User originalRecord, User updatedRecord, string userEmail, string[]? additionalPropertiesToExclude = null)
        => await UpdateWithChangeLogsAsync(originalRecord, updatedRecord, userEmail, originalRecord.SysId.ToString(), additionalPropertiesToExclude);

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
