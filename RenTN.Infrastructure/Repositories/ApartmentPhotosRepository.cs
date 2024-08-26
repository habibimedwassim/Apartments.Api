using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task CreateListAsync(List<ApartmentPhoto> apartmentPhotos)
    {
        await _dbContext.ApartmentPhotos.AddRangeAsync(apartmentPhotos);
        await _dbContext.SaveChangesAsync();
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
    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
