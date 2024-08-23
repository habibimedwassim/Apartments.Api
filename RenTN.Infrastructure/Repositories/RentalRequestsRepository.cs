using Microsoft.EntityFrameworkCore;
using RenTN.Domain.Common;
using RenTN.Domain.Entities;
using RenTN.Domain.Interfaces;
using RenTN.Infrastructure.Data;

namespace RenTN.Infrastructure.Repositories;

internal class RentalRequestsRepository(ApplicationDbContext _dbContext) : IRentalRequestsRepository
{
    public async Task<IEnumerable<RentalRequest>> GetAllAsync(string id, RentalRequestType requestType)
    {
        IQueryable<RentalRequest> query = _dbContext.RentalRequests.Include(x => x.Apartment);

        if (requestType == RentalRequestType.Sent)
        {
            query = query.Where(x => x.TenantID == id);
        }
        else if (requestType == RentalRequestType.Received)
        {
            query = query.Where(x => x.OwnerID == id);
        }

        return await query.ToListAsync();
    }
    public async Task<RentalRequest?> GetByTenantAndApartmentIdAsync(string tenantId, int apartmentId)
    {
        return await _dbContext.RentalRequests
                               .Include(x => x.Apartment)
                               .FirstOrDefaultAsync(x => x.TenantID == tenantId && x.ApartmentID == apartmentId && !x.IsDeleted);
    }

    public async Task CreateAsync(RentalRequest rentalRequest)
    {
        await _dbContext.RentalRequests.AddAsync(rentalRequest);
        await _dbContext.SaveChangesAsync();
    }
}
