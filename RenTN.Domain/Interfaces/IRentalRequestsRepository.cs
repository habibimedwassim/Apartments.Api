using RenTN.Domain.Common;
using RenTN.Domain.Entities;

namespace RenTN.Domain.Interfaces;

public interface IRentalRequestsRepository
{
    Task<IEnumerable<RentalRequest>> GetAllAsync(string id, RentalRequestType requestType);
    Task<RentalRequest?> GetByTenantAndApartmentIdAsync(string tenantId, int apartmentId);
    Task CreateAsync(RentalRequest rentalRequest);
}
