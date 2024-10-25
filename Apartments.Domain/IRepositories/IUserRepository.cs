using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.QueryFilters;
using Microsoft.EntityFrameworkCore.Storage;

namespace Apartments.Domain.IRepositories;

public interface IUserRepository
{
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<User?> GetByEmailAsync(string email, Common.VerificationCodeType verificationCodeType);
    Task<User?> GetByCinAsync(string cin);
    Task<User?> GetBySysIdAsync(int id);
    Task<User?> GetByUserIdAsync(string id);
    Task<User?> GetTenantByApartmentId(int id);
    Task RestoreUserAsync(User user, string userEmail);
    Task SoftDeleteUserAsync(User user, string userEmail);

    Task UpdateAsync(User originalRecord, User updatedRecord, string userEmail,
        string[]? additionalPropertiesToExclude = null);
    Task RemoveTempEmailAsync(string normalizedEmail);
    Task<IEnumerable<User>> GetTenantsByOwnerIdAsync(string id);
    Task<PagedModel<User>> GetUsersPagedAsync(UserQueryFilter userQueryFilter);
    Task<List<string>> GetAdmins();
}