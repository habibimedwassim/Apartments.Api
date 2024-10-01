using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.QueryFilters;

namespace Apartments.Domain.IRepositories;

public interface IApartmentRequestRepository
{
    Task<ApartmentRequest> AddApartmentRequestAsync(ApartmentRequest apartmentRequest);
    Task DeleteApartmentRequestAsync(ApartmentRequest apartmentRequest, string userEmail);
    Task<ApartmentRequest?> GetApartmentRequestByIdAsync(int id);
    Task RestoreApartmentRequestAsync(ApartmentRequest apartmentRequest, string userEmail);

    Task UpdateApartmentRequestAsync(ApartmentRequest originalRecord, ApartmentRequest updatedRecord, string userEmail,
        string[]? additionalPropertiesToExclude = null);

    Task<PagedModel<ApartmentRequest>> GetApartmentRequestsPagedAsync(
        ApartmentRequestQueryFilter apartmentRequestQueryFilter, RequestType requestType, string id);

    Task<ApartmentRequest?> GetApartmentRequestWithStatusAsync(int apartmentId, string tenantId, string type,
        string status);
}