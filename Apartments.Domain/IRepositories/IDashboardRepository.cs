using Apartments.Domain.Entities;

namespace Apartments.Domain.IRepositories;

public interface IDashboardRepository
{
    Task<AdminDashboardDetails> GetAdminDashboardDetailsAsync();
    Task<OwnerDashboardDetails> GetOwnerDashboardDetailsAsync(string id);
}
