using RenTN.Domain.Entities;

namespace RenTN.Domain.Interfaces;

public interface IApartmentPhotosRepository
{
    Task CreateAsync(ApartmentPhoto apartmentPhoto);
    Task SaveChangesAsync();
}
