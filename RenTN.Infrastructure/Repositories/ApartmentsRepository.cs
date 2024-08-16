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

    public async Task UpdateAsync(Apartment updatedApartment, List<string>? apartmentPhotoUrls)
    {
        if (apartmentPhotoUrls != null && apartmentPhotoUrls.Count > 0)
        {
            _dbContext.ApartmentPhotos.RemoveRange(updatedApartment.ApartmentPhotos);

            var photosToAdd = apartmentPhotoUrls.Select(url => new ApartmentPhoto
            {
                ApartmentID = updatedApartment.ID,
                Url = url
            }).ToList();

            await _dbContext.ApartmentPhotos.AddRangeAsync(photosToAdd);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Apartment existingApartment)
    {
        var apartmentPhotos = existingApartment.ApartmentPhotos;
        if (apartmentPhotos.Count > 0)
        {
            _dbContext.ApartmentPhotos.RemoveRange(apartmentPhotos);
        }
        _dbContext.Apartments.Remove(existingApartment);
        await _dbContext.SaveChangesAsync();
    }
}
