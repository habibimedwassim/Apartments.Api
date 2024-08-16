using RenTN.Domain.Entities;

namespace RenTN.Domain.Interfaces;

public interface IApartmentsRepository
{
    Task<int> CreateAsync(Apartment apartment);
    Task DeleteAsync(Apartment existingApartment);
    Task<IEnumerable<Apartment>> GetAllAsync();
    Task<Apartment?> GetByIdAsync(int id);
    Task SaveChangesAsync();
}
