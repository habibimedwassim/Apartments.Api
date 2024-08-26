using Microsoft.EntityFrameworkCore.Storage;
using RenTN.Domain.Entities;

namespace RenTN.Domain.Interfaces;

public interface IApartmentPhotosRepository
{
    Task CreateAsync(ApartmentPhoto apartmentPhoto);
    Task CreateListAsync(List<ApartmentPhoto> apartmentPhotos);
    Task CommitTransactionAsync(IDbContextTransaction transaction);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task RollbackTransactionAsync(IDbContextTransaction transaction);
    Task SaveChangesAsync();
}
