using RenTN.Domain.Entities;

namespace RenTN.Domain.Interfaces;

public interface IApartmentsRepository
{
    Task<IEnumerable<Apartment>> GetAllAsync();
    Task<IEnumerable<Apartment>> GetApartmentsByOwnerIdAsync(string ownerID);
    Task<Apartment?> GetByIdAsync(int id);
    Task<Apartment> CreateAsync(Apartment apartment);
    Task UpdateAsync(Apartment updatedApartment, List<string>? apartmentPhotoUrls);
    Task DeleteAsync(Apartment existingApartment);
}
