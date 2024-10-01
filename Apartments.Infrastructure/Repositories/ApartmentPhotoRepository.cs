using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Apartments.Infrastructure.Repositories;

public class ApartmentPhotoRepository(ApplicationDbContext dbContext)
    : BaseRepository<ApartmentPhoto>(dbContext), IApartmentPhotoRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task PermanentDeleteApartmentPhotosAsync(IEnumerable<ApartmentPhoto> apartmentPhotos)
    {
        _dbContext.ApartmentPhotos.RemoveRange(apartmentPhotos);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteApartmentPhotoAsync(ApartmentPhoto apartmentPhoto, string userEmail)
    {
        if (apartmentPhoto.IsDeleted) return;

        await DeleteRestoreAsync(apartmentPhoto, true, userEmail, apartmentPhoto.Id.ToString());
    }

    public async Task RestoreApartmentAsync(ApartmentPhoto apartmentPhoto, string userEmail)
    {
        if (!apartmentPhoto.IsDeleted) return;

        await DeleteRestoreAsync(apartmentPhoto, false, userEmail, apartmentPhoto.Id.ToString());
    }

    public async Task<List<ApartmentPhoto>> GetPhotosInBatchesAsync(int batchSize)
    {
        var photos = new List<ApartmentPhoto>();

        // Fetch database photos in batches
        var totalPhotosCount = await _dbContext.ApartmentPhotos.CountAsync();

        for (var i = 0; i < totalPhotosCount; i += batchSize)
        {
            var batch = await _dbContext.ApartmentPhotos
                .Skip(i)
                .Take(batchSize)
                .ToListAsync();

            photos.AddRange(batch);
        }

        return photos;
    }

    public async Task AddApartmentPhotosAsync(IEnumerable<ApartmentPhoto> apartmentPhotos)
    {
        await AddListAsync(apartmentPhotos);
    }

    public async Task<ApartmentPhoto?> GetApartmentPhotoByIdAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _dbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction)
    {
        await transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync(IDbContextTransaction transaction)
    {
        await transaction.RollbackAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}