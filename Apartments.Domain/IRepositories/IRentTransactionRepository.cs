
using Apartments.Domain.Entities;

namespace Apartments.Domain.IRepositories;

public interface IRentTransactionRepository
{
    Task<RentTransaction> AddRentTransactionAsync(RentTransaction rentTransaction);
    Task DeleteRentTransactionAsync(RentTransaction rentTransaction, string userEmail);
    Task<RentTransaction?> GetLatestRentTransactionAsync(int apartmentId, string userId);
    Task<RentTransaction?> GetRentTransactionByIdAsync(int id);
    Task<IEnumerable<RentTransaction>> GetRentTransactionsForOwnerAsync(string id);
    Task<IEnumerable<RentTransaction>> GetRentTransactionsForTenantAsync(string id);
    Task UpdateRentTransactionAsync(RentTransaction originalRecord, RentTransaction updatedRecord, string userEmail, string[]? additionalPropertiesToExclude = null);
}
