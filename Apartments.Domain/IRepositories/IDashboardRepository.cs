using Apartments.Domain.Entities;

namespace Apartments.Domain.IRepositories;

public interface IDashboardRepository
{
    Task<OwnerDashboardDetails> GetOwnerDashboardDetailsAsync(string id);
}
