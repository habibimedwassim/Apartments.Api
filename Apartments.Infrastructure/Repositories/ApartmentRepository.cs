using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Apartments.Infrastructure.Repositories;

public class ApartmentRepository(ApplicationDbContext dbContext)
    : BaseRepository<Apartment>(dbContext), IApartmentRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<PagedModel<Apartment>> GetApartmentsPagedAsync(ApartmentQueryFilter apartmentsQueryFilter, string? ownerId = null)
    {
        // Start with the base query
        var baseQuery = _dbContext.Apartments.AsQueryable();

        // Apply soft delete filter
        baseQuery = _dbContext.ApplyIsDeletedFilter(baseQuery);

        // Apply ownerId filter if provided
        if (!string.IsNullOrEmpty(ownerId))
        {
            baseQuery = baseQuery.Where(x => x.OwnerId == ownerId);
        }

        // Apply filters
        ApplyFilters(ref baseQuery, apartmentsQueryFilter);

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync();

        // Apply sorting
        ApplySorting(ref baseQuery, apartmentsQueryFilter);

        // Apply pagination
        var apartments = await baseQuery
            .Include(x => x.ApartmentPhotos)
            .Skip(AppConstants.PageSize * (apartmentsQueryFilter.pageNumber - 1))
            .Take(AppConstants.PageSize)
            .ToListAsync();

        return new PagedModel<Apartment> { Data = apartments, DataCount = totalCount };
    }
    public async Task<Apartment?> GetApartmentByTenantId(string tenantId)
    {
        return await _dbContext.Apartments.Include(x => x.ApartmentPhotos)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId);
    }
    public async Task DeleteApartmentAsync(Apartment apartment, string userEmail)
    {
        if (apartment.IsDeleted) return;

        await DeleteRestoreAsync(apartment, true, userEmail, apartment.Id.ToString());
    }

    public async Task RestoreApartmentAsync(Apartment apartment, string userEmail)
    {
        if (!apartment.IsDeleted) return;

        await DeleteRestoreAsync(apartment, false, userEmail, apartment.Id.ToString());
    }

    public async Task<Apartment?> GetApartmentByIdAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<Apartment> AddApartmentAsync(Apartment apartment)
    {
        return await AddAsync(apartment);
    }

    public async Task UpdateApartmentAsync(Apartment originalRecord, Apartment updatedRecord, string userEmail,
        string[]? additionalPropertiesToExclude = null)
    {
        await UpdateWithChangeLogsAsync(originalRecord, updatedRecord, userEmail, originalRecord.Id.ToString(),
            additionalPropertiesToExclude);
    }

    public async Task UpdateApartmentListAsync(List<Apartment> originalRecords, List<Apartment> updatedRecords,
        string userEmail, string[]? additionalPropertiesToExclude = null)
    {
        await UpdateWithChangeLogsAsync(originalRecords, updatedRecords, userEmail, additionalPropertiesToExclude);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        await transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync(IDbContextTransaction transaction)
    {
        await transaction.RollbackAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    private void ApplyFilters(ref IQueryable<Apartment> baseQuery, ApartmentQueryFilter apartmentsQueryFilter)
    {
        var titleLower = apartmentsQueryFilter.title?.ToLower();
        var cityLower = apartmentsQueryFilter.city?.ToLower();
        var streetLower = apartmentsQueryFilter.street?.ToLower();
        var postalCodeLower = apartmentsQueryFilter.postalCode?.ToLower();

        if (!string.IsNullOrEmpty(titleLower))
            baseQuery = baseQuery.Where(x => x.Title.ToLower().Contains(titleLower));

        if (!string.IsNullOrEmpty(cityLower))
            baseQuery = baseQuery.Where(x => x.City.ToLower().Contains(cityLower));

        if (!string.IsNullOrEmpty(streetLower))
            baseQuery = baseQuery.Where(x => x.Street.ToLower().Contains(streetLower));

        if (!string.IsNullOrEmpty(postalCodeLower))
            baseQuery = baseQuery.Where(x => x.PostalCode.ToLower().Contains(postalCodeLower));

        if (apartmentsQueryFilter.apartmentSize.HasValue)
            baseQuery = baseQuery.Where(x => x.Size == apartmentsQueryFilter.apartmentSize.Value);

        if (apartmentsQueryFilter.minPrice.HasValue)
            baseQuery = baseQuery.Where(x => x.RentAmount >= apartmentsQueryFilter.minPrice.Value);

        if (apartmentsQueryFilter.maxPrice.HasValue)
            baseQuery = baseQuery.Where(x => x.RentAmount <= apartmentsQueryFilter.maxPrice.Value);

        if (apartmentsQueryFilter.isOccupied.HasValue)
            baseQuery = baseQuery.Where(x => x.IsOccupied == apartmentsQueryFilter.isOccupied.Value);

        if (apartmentsQueryFilter.availableFrom.HasValue)
            baseQuery = baseQuery.Where(x => x.AvailableFrom >= apartmentsQueryFilter.availableFrom.Value);
    }

    // Helper method to apply sorting
    private void ApplySorting(ref IQueryable<Apartment> baseQuery, ApartmentQueryFilter apartmentsQueryFilter)
    {
        var sortBy = apartmentsQueryFilter.sortBy ?? nameof(Apartment.CreatedDate);
        var sortDirection = apartmentsQueryFilter.sortDirection;

        // Dictionary for mapping sort columns
        var columnSelector = new Dictionary<string, Expression<Func<Apartment, object>>>
        {
            { nameof(Apartment.CreatedDate), x => x.CreatedDate },
            { nameof(Apartment.RentAmount), x => x.RentAmount },
            { nameof(Apartment.Size), x => x.Size }
        };

        // Apply sorting
        if (columnSelector.ContainsKey(sortBy))
        {
            var selectedColumn = columnSelector[sortBy];

            baseQuery = sortDirection == SortDirection.Descending
                ? baseQuery.OrderByDescending(selectedColumn)
                : baseQuery.OrderBy(selectedColumn);
        }
    }
}