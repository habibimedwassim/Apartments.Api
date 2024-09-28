using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Domain.QueryFilters;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apartments.Infrastructure.Repositories;

public class ApartmentRequestRepository : BaseRepository<ApartmentRequest>, IApartmentRequestRepository
{
    private readonly ApplicationDbContext dbContext;

    public ApartmentRequestRepository(ApplicationDbContext _dbContext) : base(_dbContext)
    {
        dbContext = _dbContext;
    }
    public async Task<ApartmentRequest> AddApartmentRequestAsync(ApartmentRequest apartmentRequest)
        => await AddAsync(apartmentRequest);
    public async Task<ApartmentRequest?> GetApartmentRequestByIdAsync(int id) => await GetByIdAsync(id);
    public async Task UpdateApartmentRequestAsync(ApartmentRequest originalRecord, ApartmentRequest updatedRecord, string userEmail, string[]? additionalPropertiesToExclude = null)
        => await UpdateWithChangeLogsAsync(originalRecord, updatedRecord, userEmail, originalRecord.Id.ToString(), additionalPropertiesToExclude);

    public async Task DeleteApartmentRequestAsync(ApartmentRequest apartmentRequest,string userEmail)
    {
        if (apartmentRequest.IsDeleted) return;

        await DeleteRestoreAsync(apartmentRequest, true, userEmail, apartmentRequest.Id.ToString());
    }
    public async Task RestoreApartmentRequestAsync(ApartmentRequest apartmentRequest, string userEmail)
    {
        if (!apartmentRequest.IsDeleted) return;

        await DeleteRestoreAsync(apartmentRequest, false, userEmail, apartmentRequest.Id.ToString());
    }
    public async Task<ApartmentRequest?> GetApartmentRequestWithStatusAsync(int apartmentId, string tenantId, string type, string status)
    {
        return await dbContext.ApartmentRequests
                              .FirstOrDefaultAsync(x => !x.IsDeleted &&
                                                        x.ApartmentId == apartmentId &&
                                                        x.TenantId == tenantId &&
                                                        x.RequestType.Equals(type, StringComparison.CurrentCultureIgnoreCase) &&
                                                        x.Status.Equals(status, StringComparison.CurrentCultureIgnoreCase));
    }
    public async Task<ApartmentRequest?> GetApartmentRequestByApartmentIdAndUserIdAsync(int apartmentId, string tenantId)
    {
        return await dbContext.ApartmentRequests
                              .FirstOrDefaultAsync(x => x.ApartmentId == apartmentId && x.TenantId == tenantId);
    }
    public async Task<IEnumerable<ApartmentRequest>> GetApartmentRequestsByApartmentIdAsync(int apartmentId)
    {
        return await dbContext.ApartmentRequests.Where(x => x.ApartmentId == apartmentId).ToListAsync();
    }
    public async Task<PagedModel<ApartmentRequest>> GetApartmentRequestsPagedAsync(ApartmentRequestQueryFilter apartmentRequestQueryFilter, RequestType requestType, string userId)
    {
        var pageNumber = apartmentRequestQueryFilter.pageNumber;
        var typeLower = apartmentRequestQueryFilter.type.ToLower();
        var statusLower = apartmentRequestQueryFilter.status?.ToLower();
        var apartmentId = apartmentRequestQueryFilter.apartmentId;

        var baseQuery = dbContext.ApartmentRequests
                                 .IgnoreQueryFilters()
                                 .Where(x => x.RequestType.ToLower() == typeLower)
                                 .AsQueryable();

        if (requestType == RequestType.Received)
        {
            baseQuery = baseQuery.Where(x => x.OwnerId == userId);
        }
        else if (requestType == RequestType.Sent)
        {
            baseQuery = baseQuery.Where(x => x.TenantId == userId);
        }

        if (apartmentId != null)
        {
            baseQuery = baseQuery.Where(x => x.ApartmentId == apartmentId);
        }

        if (!string.IsNullOrEmpty(statusLower))
        {
            baseQuery = baseQuery.Where(x => x.Status.ToLower() == statusLower);
        }

        // Get total count before pagination
        var totalCount = await baseQuery.CountAsync();

        // Default sorting if no sortBy is provided
        var sortBy = apartmentRequestQueryFilter.sortBy ?? nameof(ApartmentRequest.CreatedDate);
        var sortDirection = apartmentRequestQueryFilter.sortDirection;

        // Dictionary for mapping sort columns
        var columnSelector = new Dictionary<string, Expression<Func<ApartmentRequest, object>>>
        {
            { nameof(ApartmentRequest.CreatedDate), x => x.CreatedDate },
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
}
