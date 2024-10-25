using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.QueryFilters;

namespace Apartments.Domain.IRepositories;

public interface IRentTransactionRepository
{
    Task<RentTransaction> AddRentTransactionAsync(RentTransaction rentTransaction);
    Task DeleteRentTransactionAsync(RentTransaction rentTransaction, string userEmail);
    Task DeletePendingRentTransactionsAsync(RentTransaction rentTransaction);
    Task<RentTransaction?> GetLatestRentTransactionAsync(int apartmentId, string userId);
    Task<RentTransaction?> GetRentTransactionByIdAsync(int id);
    Task<IEnumerable<RentTransaction>> GetRentTransactionsForUserAsync(string id, string? role);

    Task UpdateRentTransactionAsync(RentTransaction originalRecord, RentTransaction updatedRecord, string userEmail,
        string[]? additionalPropertiesToExclude = null);
    Task<PagedModel<RentTransaction>> GetRentTransactionsPagedAsync(RentTransactionQueryFilter filter, string userId, string? ownerRole);
    Task<bool> CheckExistingTransactionAsync(int apartmentId, string userId, DateOnly dateFrom, DateOnly dateTo);
    Task<List<RentTransaction>> GetTransactionsWithDueDate(DateOnly dateOnly);
}