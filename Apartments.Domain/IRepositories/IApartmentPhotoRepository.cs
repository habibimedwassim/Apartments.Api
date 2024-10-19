using Apartments.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Apartments.Domain.IRepositories;

public interface IApartmentPhotoRepository
{
    Task<List<ApartmentPhoto>> GetPhotosInBatchesAsync(int batchSize);
    Task AddApartmentPhotosAsync(IEnumerable<ApartmentPhoto> apartmentPhotos);
    Task DeleteApartmentPhotoAsync(ApartmentPhoto apartmentPhoto);
    Task PermanentDeleteApartmentPhotosAsync(IEnumerable<ApartmentPhoto> apartmentPhotos);
    Task RestoreApartmentAsync(ApartmentPhoto apartmentPhoto, string userEmail);

    Task CommitTransactionAsync(IDbContextTransaction transaction);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task RollbackTransactionAsync(IDbContextTransaction transaction);
    Task SaveChangesAsync();
}