using Microsoft.EntityFrameworkCore;
using RenTN.Domain.Entities;
using RenTN.Domain.Interfaces;
using RenTN.Infrastructure.Data;

namespace RenTN.Infrastructure.Repositories;

internal class ApartmentsRepository(ApplicationDbContext _dbContext) : IApartmentsRepository
{
    public async Task<IEnumerable<Apartment>> GetAllAsync()
    {
        var apartments = await _dbContext.Apartments.Include(x => x.ApartmentPhotos).ToListAsync();
        return apartments;
    }

    public async Task<Apartment?> GetByIdAsync(int id)
    {
        var apartment = await _dbContext.Apartments.Include(x => x.ApartmentPhotos).FirstOrDefaultAsync(x => x.ID == id);
        return apartment;
    }
    public async Task<int> CreateAsync(Apartment apartment)
    {
        await _dbContext.Apartments.AddAsync(apartment);
        await _dbContext.SaveChangesAsync();
        return apartment.ID;
    }
}
