using Apartments.Domain.Entities;

namespace Apartments.Domain.IRepositories;

public interface IUserRepository
{
    Task<User?> GetBySysIdAsync(int id);
    Task<User?> GetByUserIdAsync(string id);
    Task<User?> GetTenantByApartmentId(int id);
    Task RestoreUserAsync(User user, string userEmail);
    Task SoftDeleteUserAsync(User user, string userEmail);

    Task UpdateAsync(User originalRecord, User updatedRecord, string userEmail,
        string[]? additionalPropertiesToExclude = null);
}