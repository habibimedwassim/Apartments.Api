using RenTN.Domain.Entities;
using RenTN.Domain.Interfaces;
using RenTN.Infrastructure.Data;

namespace RenTN.Infrastructure.Repositories;

internal class ApartmentPhotosRepository(ApplicationDbContext _dbContext) : IApartmentPhotosRepository
{
    public async Task CreateAsync(ApartmentPhoto apartmentPhoto)
    {
        await _dbContext.ApartmentPhotos.AddAsync(apartmentPhoto);
        await _dbContext.SaveChangesAsync();
    }

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
