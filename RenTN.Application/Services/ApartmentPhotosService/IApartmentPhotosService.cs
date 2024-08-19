using RenTN.Application.DTOs.ApartmentPhotoDTOs;

namespace RenTN.Application.Services.ApartmentPhotosService;

public interface IApartmentPhotosService
{
    Task DeleteApartmentPhoto(int apartmentId, int photoId);
    Task<ApartmentPhotoDTO> GetByIdForApartment(int apartmentId, int id);
    Task<IEnumerable<ApartmentPhotoDTO>> GetAllApartmentPhotos(int apartmentId);
    Task UpdateApartmentPhoto(int apartmentId, ApartmentPhotoDTO updateApartmentPhotoDTO);
    Task AddApartmentPhoto(int apartmentId, CreateUpdateApartmentPhotoDTO createApartmentPhotoDTO);
}
