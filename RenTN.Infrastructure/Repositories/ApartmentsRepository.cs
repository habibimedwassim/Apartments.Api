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
    public async Task<Apartment> CreateAsync(Apartment apartment)
    {
        await _dbContext.Apartments.AddAsync(apartment);
        await _dbContext.SaveChangesAsync();
        return apartment;
    }
    public async Task DeleteAsync(Apartment existingApartment)
    {
        var apartmentPhotos = existingApartment.ApartmentPhotos;
        if (apartmentPhotos.Count > 0)
        {
            foreach (var photo in apartmentPhotos)
            {
                photo.IsDeleted = true;
            }
        }
        existingApartment.IsDeleted = true;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Apartment>> GetApartmentsByOwnerIdAsync(string ownerID)
    {
        var apartments = await _dbContext.Apartments
                                         .Include(x => x.ApartmentPhotos)
                                         .Where(x => x.OwnerID == ownerID)
                                         .ToListAsync();
        return apartments;
    }

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
