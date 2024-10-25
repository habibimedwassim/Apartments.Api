using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apartments.Infrastructure.Repositories;

public class RentTransactionRepository(ApplicationDbContext dbContext)
    : BaseRepository<RentTransaction>(dbContext), IRentTransactionRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<List<RentTransaction>> GetTransactionsWithDueDate(DateOnly dueDate)
    {
        return await _dbContext.RentTransactions
            .Where(x => x.DateTo == dueDate &&
                        x.Status == RequestStatus.Paid &&
                        !x.IsDeleted)
            .ToListAsync();
    }
    public async Task<RentTransaction?> GetLatestRentTransactionAsync(int apartmentId, string userId)
    {
        return await _dbContext.RentTransactions
            .Where(x => x.ApartmentId == apartmentId && 
                        x.TenantId == userId && 
                        (x.Status == RequestStatus.Paid || x.Status == RequestStatus.Late) &&
                        !x.IsDeleted)
            .OrderByDescending(x => x.DateTo)
            .FirstOrDefaultAsync();
    }
    public async Task<bool> CheckExistingTransactionAsync(int apartmentId, string userId, DateOnly dateFrom, DateOnly dateTo)
    {
        return await _dbContext.RentTransactions
            .AnyAsync(x => x.ApartmentId == apartmentId &&
                           x.TenantId == userId &&
                           (x.DateFrom < dateTo &&  x.DateTo > dateFrom) &&
                           !x.IsDeleted);
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
    public async Task<IEnumerable<RentTransaction>> GetRentTransactionsForUserAsync(string id, string? ownerRole)
    {
        var query = _dbContext.RentTransactions
            .Include(x => x.Apartment)
            .ThenInclude(x => x.Owner)
            .Include(x => x.Tenant)
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
    public async Task<PagedModel<RentTransaction>> GetRentTransactionsPagedAsync(RentTransactionQueryFilter filter, string userId, string? ownerRole)
    {
        var baseQuery = _dbContext.RentTransactions
            .Include(x => x.Apartment)
            .Include(x => x.Tenant)
            .Include(x => x.Owner)
            .AsQueryable();

        // Apply soft delete filter
        baseQuery = _dbContext.ApplyIsDeletedFilter(baseQuery);

        if (!string.IsNullOrEmpty(ownerRole) && ownerRole == UserRoles.Owner)
        {
            baseQuery = baseQuery.Where(x => x.OwnerId == userId);
        }
        else
        {
            baseQuery = baseQuery.Where(x => x.TenantId == userId);
        }

        // Apply filters
        ApplyFilters(ref baseQuery, filter);

        // Get the total count before pagination
        var totalCount = await baseQuery.CountAsync();

        // Apply sorting
        ApplySorting(ref baseQuery, filter);

        // Apply pagination
        var rentTransactions = await baseQuery
            .Skip(AppConstants.PageSize * (filter.PageNumber - 1))
            .Take(AppConstants.PageSize)
            .ToListAsync();

        return new PagedModel<RentTransaction> { Data = rentTransactions, DataCount = totalCount };
    }

    private void ApplyFilters(ref IQueryable<RentTransaction> baseQuery, RentTransactionQueryFilter filter)
    {
        if (!string.IsNullOrEmpty(filter.Status))
            baseQuery = baseQuery.Where(x => x.Status == filter.Status);

        if (filter.DateFrom.HasValue)
            baseQuery = baseQuery.Where(x => x.DateFrom >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            baseQuery = baseQuery.Where(x => x.DateTo <= filter.DateTo.Value);
    }

    private void ApplySorting(ref IQueryable<RentTransaction> baseQuery, RentTransactionQueryFilter filter)
    {
        var sortBy = filter.SortBy ?? nameof(RentTransaction.CreatedDate);
        var sortDirection = filter.SortDirection;

        var columnSelector = new Dictionary<string, Expression<Func<RentTransaction, object>>>
    {
        { nameof(RentTransaction.CreatedDate), x => x.CreatedDate },
        { nameof(RentTransaction.RentAmount), x => x.RentAmount },
        { nameof(RentTransaction.DateFrom), x => x.DateFrom }
    };

        if (columnSelector.ContainsKey(sortBy))
        {
            var selectedColumn = columnSelector[sortBy];
            baseQuery = sortDirection == SortDirection.Descending
                ? baseQuery.OrderByDescending(selectedColumn)
                : baseQuery.OrderBy(selectedColumn);
        }
    }
}