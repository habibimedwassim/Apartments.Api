using Microsoft.EntityFrameworkCore.Storage;
using RenTN.Domain.Entities;

namespace RenTN.Domain.Interfaces;

public interface IApartmentsRepository
{
    Task<IEnumerable<Apartment>> GetAllAsync();
    Task<IEnumerable<Apartment>> GetApartmentsByOwnerIdAsync(string ownerID);
    Task<Apartment?> GetByIdAsync(int id);
    Task<Apartment> CreateAsync(Apartment apartment);
    Task DeleteAsync(Apartment existingApartment);
    Task CommitTransactionAsync(IDbContextTransaction transaction);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task RollbackTransactionAsync(IDbContextTransaction transaction);
    Task SaveChangesAsync();
}
