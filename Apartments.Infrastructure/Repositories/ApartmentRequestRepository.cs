using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apartments.Infrastructure.Repositories;

public class ApartmentRequestRepository(ApplicationDbContext dbContext)
    : BaseRepository<ApartmentRequest>(dbContext), IApartmentRequestRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<ApartmentRequest> AddApartmentRequestAsync(ApartmentRequest apartmentRequest)
    {
        return await AddAsync(apartmentRequest);
    }

    public async Task<ApartmentRequest?> GetApartmentRequestByIdAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    public async Task UpdateApartmentRequestAsync(ApartmentRequest originalRecord, ApartmentRequest updatedRecord,
        string userEmail, string[]? additionalPropertiesToExclude = null)
    {
        await UpdateWithChangeLogsAsync(originalRecord, updatedRecord, userEmail, originalRecord.Id.ToString(),
            additionalPropertiesToExclude);
    }

    public async Task DeleteApartmentRequestAsync(ApartmentRequest apartmentRequest, string userEmail)
    {
        if (apartmentRequest.IsDeleted) return;

        await DeleteRestoreAsync(apartmentRequest, true, userEmail, apartmentRequest.Id.ToString());
    }

    public async Task RestoreApartmentRequestAsync(ApartmentRequest apartmentRequest, string userEmail)
    {
        if (!apartmentRequest.IsDeleted) return;

        await DeleteRestoreAsync(apartmentRequest, false, userEmail, apartmentRequest.Id.ToString());
    }

    public async Task<ApartmentRequest?> GetApartmentRequestWithStatusAsync(int apartmentId, string tenantId,
        string type)
    {
        return await _dbContext.ApartmentRequests
            .FirstOrDefaultAsync(x => !x.IsDeleted &&
                                      x.ApartmentId == apartmentId &&
                                      x.TenantId == tenantId &&
                                      x.RequestType.ToLower() == type.ToLower() &&
                                      (x.Status.ToLower() == RequestStatus.Pending.ToLower() ||
                                      x.Status.ToLower() == RequestStatus.MeetingScheduled.ToLower()));
    }

    public async Task<ApartmentRequest?> GetApartmentRequestByApartmentIdAndUserIdAsync(int apartmentId,
        string tenantId)
    {
        return await _dbContext.ApartmentRequests
            .FirstOrDefaultAsync(x => x.ApartmentId == apartmentId && x.TenantId == tenantId);
    }

    public async Task<IEnumerable<ApartmentRequest>> GetApartmentRequestsByApartmentIdAsync(int apartmentId)
    {
        return await _dbContext.ApartmentRequests.Where(x => x.ApartmentId == apartmentId).ToListAsync();
    }

    public async Task<PagedModel<ApartmentRequest>> GetApartmentRequestsPagedAsync(
        ApartmentRequestPagedQueryFilter apartmentRequestQueryFilter, RequestType requestType, string userId)
    {
        var pageNumber = apartmentRequestQueryFilter.pageNumber;
        var typeLower = apartmentRequestQueryFilter.type.ToLower();
        var statusLower = apartmentRequestQueryFilter.status?.ToLower();
        var apartmentId = apartmentRequestQueryFilter.apartmentId;

        var baseQuery = _dbContext.ApartmentRequests
            .Include(x => x.Tenant)
            .Include(x => x.Apartment)
            .Where(x => x.RequestType.ToLower() == typeLower && x.IsDeleted == false)
            .AsQueryable();

        if (requestType == RequestType.Received)
            baseQuery = baseQuery.Where(x => x.OwnerId == userId);
        else if (requestType == RequestType.Sent) baseQuery = baseQuery.Where(x => x.TenantId == userId);

        if (apartmentId != null) baseQuery = baseQuery.Where(x => x.ApartmentId == apartmentId);

        if (!string.IsNullOrEmpty(statusLower)) baseQuery = baseQuery.Where(x => x.Status.ToLower() == statusLower);

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync();

        // Default sorting if no sortBy is provided
        var sortBy = apartmentRequestQueryFilter.sortBy ?? nameof(ApartmentRequest.CreatedDate);
        var sortDirection = apartmentRequestQueryFilter.sortDirection;

        // Dictionary for mapping sort columns
        var columnSelector = new Dictionary<string, Expression<Func<ApartmentRequest, object>>>
        {
            { nameof(ApartmentRequest.CreatedDate), x => x.CreatedDate }
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
        var apartmentRequests = await baseQuery.Skip(AppConstants.PageSize * (pageNumber - 1))
            .Take(AppConstants.PageSize)
            .ToListAsync();

        return new PagedModel<ApartmentRequest> { Data = apartmentRequests, DataCount = totalCount };
    }
    public async Task<IEnumerable<ApartmentRequest>> GetApartmentRequestsAsync(ApartmentRequestQueryFilter apartmentRequestQueryFilter,
        RequestType requestType, string userId)
    {
        var typeLower = apartmentRequestQueryFilter.type.ToLower();
        var statusLower = apartmentRequestQueryFilter.status?.ToLower();
        var apartmentId = apartmentRequestQueryFilter.apartmentId;

        var baseQuery = _dbContext.ApartmentRequests
            .Include(x => x.Tenant)
            .Where(x => x.RequestType.ToLower() == typeLower)
            .AsQueryable();

        if (requestType == RequestType.Received)
            baseQuery = baseQuery.Where(x => x.OwnerId == userId);
        else if (requestType == RequestType.Sent) baseQuery = baseQuery.Where(x => x.TenantId == userId);

        if (apartmentId != null) baseQuery = baseQuery.Where(x => x.ApartmentId == apartmentId);

        if (!string.IsNullOrEmpty(statusLower)) baseQuery = baseQuery.Where(x => x.Status.ToLower() == statusLower);

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync();

        // Default sorting if no sortBy is provided
        var sortBy = apartmentRequestQueryFilter.sortBy ?? nameof(ApartmentRequest.CreatedDate);
        var sortDirection = apartmentRequestQueryFilter.sortDirection;

        // Dictionary for mapping sort columns
        var columnSelector = new Dictionary<string, Expression<Func<ApartmentRequest, object>>>
        {
            { nameof(ApartmentRequest.CreatedDate), x => x.CreatedDate }
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
        var apartmentRequests = await baseQuery
            .ToListAsync();

        return apartmentRequests;
    }

    public async Task CancelRemainingRequests(ApartmentRequest apartmentRequest)
    {
        var remainingRequests = await _dbContext.ApartmentRequests
                                                .Where(x => x.TenantId == apartmentRequest.TenantId &&
                                                            x.OwnerId == apartmentRequest.OwnerId &&
                                                            x.Id != apartmentRequest.Id &&
                                                            (x.Status == RequestStatus.Pending || x.Status == RequestStatus.MeetingScheduled))
                                                .ToListAsync();

        foreach(var request in remainingRequests)
        {
            request.Status = RequestStatus.Cancelled;
            request.IsDeleted = true;
        }
        await _dbContext.SaveChangesAsync();
    }
}