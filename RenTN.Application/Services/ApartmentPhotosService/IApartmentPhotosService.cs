using RenTN.Application.DTOs.ApartmentPhotoDTOs;

namespace RenTN.Application.Services.ApartmentPhotosService;

public interface IApartmentPhotosService
{
    Task<IEnumerable<ApartmentPhotoDTO>> GetAllApartmentPhotos(int apartmentId);
    Task<ApartmentPhotoDTO> GetByIdForApartment(int apartmentId, int id);
}
