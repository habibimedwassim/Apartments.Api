using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Apartments.Infrastructure.Repositories;

public class RentTransactionRepository(ApplicationDbContext dbContext)
    : BaseRepository<RentTransaction>(dbContext), IRentTransactionRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<RentTransaction?> GetLatestRentTransactionAsync(int apartmentId, string userId)
    {
        return await _dbContext.RentTransactions
            .Where(x => x.ApartmentId == apartmentId && 
                        x.TenantId == userId && 
                        x.Status == RequestStatus.Paid &&
                        !x.IsDeleted)
            .OrderByDescending(x => x.DateTo)
            .FirstOrDefaultAsync();
    }
    public async Task<IEnumerable<RentTransaction>> GetRentTransactionsForUserAsync(string id, string? ownerRole)
    {
        var query = _dbContext.RentTransactions
            .Include(x => x.Apartment)
            .ThenInclude(x => x.Owner)
            .Include(x => x.Apartment.ApartmentPhotos).AsQueryable();

        if (!string.IsNullOrEmpty(ownerRole) && ownerRole == UserRoles.Owner)
        {
            query = query.Where(x => x.OwnerId == id);
        }
        else
        {
            query = query.Where(x => x.TenantId == id);
        }

        return await query.ToListAsync();
    }
    public async Task<RentTransaction> AddRentTransactionAsync(RentTransaction rentTransaction)
    {
        return await AddAsync(rentTransaction);
    }

    public async Task<RentTransaction?> GetRentTransactionByIdAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    public async Task UpdateRentTransactionAsync(RentTransaction originalRecord, RentTransaction updatedRecord,
        string userEmail, string[]? additionalPropertiesToExclude = null)
    {
        await UpdateWithChangeLogsAsync(originalRecord, updatedRecord, userEmail, originalRecord.Id.ToString(),
            additionalPropertiesToExclude);
    }

    public async Task DeleteRentTransactionAsync(RentTransaction rentTransaction, string userEmail)
    {
        if (rentTransaction.IsDeleted) return;

        await DeleteRestoreAsync(rentTransaction, true, userEmail, rentTransaction.Id.ToString());
    }

    public async Task DeletePendingRentTransactionsAsync(RentTransaction rentTransaction)
    {
        var transactionsToRemove = await _dbContext.RentTransactions
                                                   .Where(x => x.ApartmentId == rentTransaction.ApartmentId &&
                                                               x.OwnerId == rentTransaction.OwnerId &&
                                                               (x.Status == RequestStatus.Pending || 
                                                               x.Status == RequestStatus.MeetingScheduled))
                                                   .ToListAsync();

        _dbContext.RentTransactions.RemoveRange(transactionsToRemove);
        await _dbContext.SaveChangesAsync();
    }
}