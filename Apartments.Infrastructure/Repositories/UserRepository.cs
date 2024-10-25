using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
    public async Task<User?> GetByEmailAsync(string email, VerificationCodeType verificationCodeType)
    {
        if (verificationCodeType == VerificationCodeType.NewEmail) 
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.TempEmail == email);
        }

        return await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
    }
    public async Task<User?> GetByCinAsync(string cin)
    {
        return await _dbContext.Users.SingleOrDefaultAsync(x => x.CIN == cin);
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
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task RemoveTempEmailAsync(string tempEmail)
    {
        var normalizedEmail = tempEmail.ToUpper();

        await _dbContext.Users
            .Where(x => x.TempEmail != null && x.TempEmail.ToUpper() == normalizedEmail)
            .ForEachAsync(user => 
                { 
                    user.TempEmail = null;
                });

        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetTenantsByOwnerIdAsync(string ownerId)
    {
        return await _dbContext.Apartments
            .Where(apartment => apartment.OwnerId == ownerId && apartment.TenantId != null)
            .Include(apartment => apartment.Tenant)
            .Select(apartment => apartment.Tenant!)
            .ToListAsync()
            ?? Enumerable.Empty<User>();
    }
    public async Task<PagedModel<User>> GetUsersPagedAsync(UserQueryFilter userQueryFilter)
    {
        IQueryable<User> baseQuery;

        if (!string.IsNullOrEmpty(userQueryFilter.Role))
        {
            if (userQueryFilter.Role == UserRoles.User)
            {
                // Fetch tenants from Apartments table where IsOccupied == true and TenantId is not null
                baseQuery = _dbContext.Apartments
                    .Include(x => x.Tenant)
                    .Where(a => a.IsOccupied && a.TenantId != null)
                    .Select(a => a.Tenant!)
                    .Where(t => t != null)
                    .AsQueryable();
            }
            else
            {
                // Fetch users based on specified role (Owner or Admin)
                baseQuery = _dbContext.Users
                    .Where(u => u.Role == userQueryFilter.Role);
            }
        }
        else
        {
            // Fetch all Owners and Admins from Users table
            var userRolesQuery = _dbContext.Users
                .Where(u => u.Role == UserRoles.Admin || u.Role == UserRoles.Owner);

            // Fetch tenants from Apartments table where IsOccupied == true and TenantId is not null
            var tenantQuery = _dbContext.Apartments
                .Include(x => x.Tenant)
                .Where(a => a.IsOccupied && a.TenantId != null)
                .Select(a => a.Tenant!)
                .Where(t => t != null);

            // Combine both queries
            baseQuery = userRolesQuery
                .Concat(tenantQuery) // Use Concat instead of Union to avoid type mismatch
                .AsQueryable();
        }

        // Apply search term filter if provided
        if (!string.IsNullOrEmpty(userQueryFilter.SearchTerm))
        {
            baseQuery = baseQuery.Where(x =>
                x.FirstName.Contains(userQueryFilter.SearchTerm) ||
                x.LastName.Contains(userQueryFilter.SearchTerm) ||
                (x.Email != null && x.Email.Contains(userQueryFilter.SearchTerm)));
        }

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync();

        // Apply sorting
        if (!string.IsNullOrEmpty(userQueryFilter.SortBy))
        {
            baseQuery = userQueryFilter.SortDescending
                ? baseQuery.OrderByDescending(x => EF.Property<object>(x, userQueryFilter.SortBy))
                : baseQuery.OrderBy(x => EF.Property<object>(x, userQueryFilter.SortBy));
        }

        // Apply pagination
        var users = await baseQuery
            .Skip(AppConstants.PageSize * (userQueryFilter.PageNumber - 1))
            .Take(AppConstants.PageSize)
            .ToListAsync();

        return new PagedModel<User> { Data = users, DataCount = totalCount };
    }

    public async Task<List<string>> GetAdmins()
    {
        return await _dbContext.Users.Where(x => x.Role == UserRoles.Admin && x.IsDeleted == false)
                    .Select(x => x.Id).ToListAsync();
    }
}