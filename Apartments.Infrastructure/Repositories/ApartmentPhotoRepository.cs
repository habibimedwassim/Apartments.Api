using Apartments.Domain.Entities;
using Apartments.Domain.IRepositories;
using Apartments.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Apartments.Infrastructure.Repositories;

public class ApartmentPhotoRepository : BaseRepository<ApartmentPhoto>, IApartmentPhotoRepository
{
    private readonly ApplicationDbContext dbContext;

    public ApartmentPhotoRepository(ApplicationDbContext _dbContext) : base(_dbContext)
    {
        dbContext = _dbContext;
    }
    public async Task PermanentDeleteApartmentPhotosAsync(IEnumerable<ApartmentPhoto> apartmentPhotos)
    {
        dbContext.ApartmentPhotos.RemoveRange(apartmentPhotos);
        await dbContext.SaveChangesAsync();
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
        var totalPhotosCount = await dbContext.ApartmentPhotos.CountAsync();

        for (int i = 0; i < totalPhotosCount; i += batchSize)
        {
            var batch = await dbContext.ApartmentPhotos
                .Skip(i)
                .Take(batchSize)
                .ToListAsync();

            photos.AddRange(batch);
        }

        return photos;
    }
    public async Task AddApartmentPhotosAsync(IEnumerable<ApartmentPhoto> apartmentPhotos) => await AddListAsync(apartmentPhotos);
    public async Task<ApartmentPhoto?> GetApartmentPhotoByIdAsync(int id) => await GetByIdAsync(id);
    public async Task<IDbContextTransaction> BeginTransactionAsync() => await dbContext.Database.BeginTransactionAsync();
    public async Task CommitTransactionAsync(IDbContextTransaction transaction) => await transaction.CommitAsync();
    public async Task RollbackTransactionAsync(IDbContextTransaction transaction) => await transaction.RollbackAsync();
    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}
