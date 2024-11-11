using Apartments.Domain.Common;
using Apartments.Domain.Entities;
using Apartments.Domain.QueryFilters;
using Microsoft.EntityFrameworkCore.Storage;

namespace Apartments.Domain.IRepositories;

public interface IApartmentRepository
{
    Task<PagedModel<Apartment>> GetApartmentsPagedAsync(ApartmentQueryFilter apartmentsQueryFilter, string? ownerId = null);
    Task<IEnumerable<Apartment>> GetOwnedApartmentsAsync(string ownerId);
    Task<Apartment?> GetApartmentByTenantId(string tenantId);
    Task<Apartment?> GetApartmentByIdAsync(int id);
    Task<Apartment> AddApartmentAsync(Apartment apartment);

    Task UpdateApartmentAsync(Apartment originalRecord, Apartment updatedRecord, string userEmail,
        string[]? additionalPropertiesToExclude = null);

    Task UpdateApartmentListAsync(List<Apartment> originalRecords, List<Apartment> updatedRecords, string userEmail,
        string[]? additionalPropertiesToExclude = null);

    Task DeleteRestoreApartmentAsync(Apartment apartment, string userEmail);

    Task CommitTransactionAsync(IDbContextTransaction transaction);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task RollbackTransactionAsync(IDbContextTransaction transaction);
    Task SaveChangesAsync();
    Task DeleteApartmentPermanentlyAsync(Apartment apartment);
    Task<IEnumerable<Apartment>> GetApartmentsList(List<int> apartmentsIds);
}