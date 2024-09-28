using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Apartments.Infrastructure.Repositories;

public class RentTransactionRepository : BaseRepository<RentTransaction>, IRentTransactionRepository
{
    private readonly ApplicationDbContext dbContext;
    public RentTransactionRepository(ApplicationDbContext _dbContext) : base(_dbContext)
    {
        dbContext = _dbContext;
    }
    public async Task<RentTransaction?> GetLatestRentTransactionAsync(int apartmentId, string userId)
    {
        return await dbContext.RentTransactions
                              .Where(x => x.ApartmentId == apartmentId && x.TenantId == userId)
                              .OrderByDescending(x => x.DateTo)
                              .FirstOrDefaultAsync();
    }
    public async Task<IEnumerable<RentTransaction>> GetRentTransactionsForOwnerAsync(string id)
    {
        var rentHistories = await dbContext.RentTransactions
                                           .Include(x => x.Apartment)
                                           .Include(x => x.Apartment.ApartmentPhotos)
                                           .Where(x => x.OwnerId == id).ToListAsync();
        return rentHistories;
    }
    public async Task<IEnumerable<RentTransaction>> GetRentTransactionsForTenantAsync(string id)
    {
        var baseQuery = dbContext.RentTransactions.IgnoreQueryFilters().AsQueryable();

        var rentHistories = await dbContext.RentTransactions
                                           .Include(x => x.Apartment)
                                           .Include(x => x.Apartment.ApartmentPhotos)
                                           .Where(x => x.TenantId == id).ToListAsync();
        return rentHistories;
    }

    public async Task<RentTransaction> AddRentTransactionAsync(RentTransaction rentTransaction) 
        => await AddAsync(rentTransaction);
    public async Task<RentTransaction?> GetRentTransactionByIdAsync(int id) => await GetByIdAsync(id);
    public async Task UpdateRentTransactionAsync(RentTransaction originalRecord, RentTransaction updatedRecord, string userEmail, string[]? additionalPropertiesToExclude = null)
        => await UpdateWithChangeLogsAsync(originalRecord, updatedRecord, userEmail, originalRecord.Id.ToString(), additionalPropertiesToExclude);
    public async Task DeleteRentTransactionAsync(RentTransaction rentTransaction, string userEmail)
    {
        if (rentTransaction.IsDeleted) return;

        await DeleteRestoreAsync(rentTransaction, true, userEmail, rentTransaction.Id.ToString());
    }
}
