using RenTN.Domain.Entities;

namespace RenTN.Domain.Interfaces;

public interface IApartmentsRepository
{
    Task<IEnumerable<Apartment>> GetAllAsync();
    Task<Apartment?> GetByIdAsync(int id);
    Task<int> CreateAsync(Apartment apartment);
    Task UpdateAsync(Apartment updatedApartment, List<string>? apartmentPhotoUrls);
    Task DeleteAsync(Apartment existingApartment);
}
