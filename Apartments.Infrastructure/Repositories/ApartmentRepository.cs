using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.Exceptions;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Apartments.Infrastructure.Repositories;

public class ApartmentRepository : BaseRepository<Apartment>, IApartmentRepository
{
    private readonly ApplicationDbContext dbContext;

    public ApartmentRepository(ApplicationDbContext _dbContext) : base(_dbContext)
    {
        dbContext = _dbContext;
    }

    public async Task<PagedModel<Apartment>> GetApartmentsPagedAsync(ApartmentQueryFilter apartmentsQueryFilter)
    {
        var cityLower = apartmentsQueryFilter.city?.ToLower();
        var streetLower = apartmentsQueryFilter.street?.ToLower();
        var postalCodeLower = apartmentsQueryFilter.postalCode?.ToLower();
        var pageNumber = apartmentsQueryFilter.pageNumber;

        var baseQuery = dbContext.Apartments.AsQueryable();

        baseQuery = dbContext.ByPassIsDeletedFilter(baseQuery);

        // Apply filters
        if (!string.IsNullOrEmpty(cityLower))
        {
            baseQuery = baseQuery.Where(x => x.City.ToLower().Contains(cityLower));
        }

        if (!string.IsNullOrEmpty(streetLower))
        {
            baseQuery = baseQuery.Where(x => x.Street.ToLower().Contains(streetLower));
        }

        if (!string.IsNullOrEmpty(postalCodeLower))
        {
            baseQuery = baseQuery.Where(x => x.PostalCode.ToLower().Contains(postalCodeLower));
        }

        if (apartmentsQueryFilter.apartmentSize.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.Size == apartmentsQueryFilter.apartmentSize.Value);
        }

        if (apartmentsQueryFilter.minPrice.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.RentAmount >= apartmentsQueryFilter.minPrice.Value);
        }

        if (apartmentsQueryFilter.maxPrice.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.RentAmount <= apartmentsQueryFilter.maxPrice.Value);
        }

        if (apartmentsQueryFilter.isOccupied.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.IsOccupied == apartmentsQueryFilter.isOccupied.Value);
        }

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync();

        // Default sorting if no sortBy is provided
        var sortBy = apartmentsQueryFilter.sortBy ?? nameof(Apartment.CreatedDate);
        var sortDirection = apartmentsQueryFilter.sortDirection;

        // Dictionary for mapping sort columns
        var columnSelector = new Dictionary<string, Expression<Func<Apartment, object>>>
        {
            { nameof(Apartment.CreatedDate), x => x.CreatedDate },
            { nameof(Apartment.RentAmount), x => x.RentAmount },
            { nameof(Apartment.Size), x => x.Size },
        };

        // Apply sorting
        if (columnSelector.ContainsKey(sortBy))
        {
            var selectedColumn = columnSelector[sortBy];

            baseQuery = sortDirection == SortDirection.Descending
                        ? baseQuery.OrderByDescending(selectedColumn)
                        : baseQuery.OrderBy(selectedColumn);
        }

        // Apply pagination
        var apartments = await baseQuery
            .Include(x => x.ApartmentPhotos)
            .Skip(AppConstants.PageSize * (pageNumber - 1))
            .Take(AppConstants.PageSize)
            .ToListAsync();

        return new PagedModel<Apartment> { Data = apartments, DataCount = totalCount };
    }

    public async Task<IEnumerable<Apartment>> GetOwnedApartmentsAsync(string ownerId)
    {
        return await dbContext.Apartments.Include(x => x.ApartmentPhotos)
                                         .Where(x => x.OwnerId == ownerId)
                                         .OrderBy(x => x.CreatedDate)
                                         .ToListAsync();
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

    public async Task<Apartment?> GetApartmentByIdAsync(int id) => await GetByIdAsync(id);
    public async Task<Apartment> AddApartmentAsync(Apartment apartment) => await AddAsync(apartment);
    public async Task UpdateApartmentAsync(Apartment originalRecord, Apartment updatedRecord, string userEmail, string[]? additionalPropertiesToExclude = null) 
        => await UpdateWithChangeLogsAsync(originalRecord, updatedRecord, userEmail, originalRecord.Id.ToString(), additionalPropertiesToExclude);
    public async Task UpdateApartmentListAsync(List<Apartment> originalRecords, List<Apartment> updatedRecords, string userEmail, string[]? additionalPropertiesToExclude = null)
        => await UpdateWithChangeLogsAsync(originalRecords, updatedRecords, userEmail, additionalPropertiesToExclude);

    public async Task<IDbContextTransaction> BeginTransactionAsync() => await dbContext.Database.BeginTransactionAsync();
    public async Task CommitTransactionAsync(IDbContextTransaction transaction) => await transaction.CommitAsync();
    public async Task RollbackTransactionAsync(IDbContextTransaction transaction) => await transaction.RollbackAsync();
    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();

    public Task<User?> GetApartmentTenant(int id)
    {
        throw new NotImplementedException();
    }
}
