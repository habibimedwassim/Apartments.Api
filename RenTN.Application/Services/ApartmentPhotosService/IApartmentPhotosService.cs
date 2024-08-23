using RenTN.Application.DTOs.ApartmentPhotoDTOs;
using RenTN.Application.Utilities;

namespace RenTN.Application.Services.ApartmentPhotosService;

public interface IApartmentPhotosService
{
    Task<ApplicationResponse> DeleteApartmentPhoto(int apartmentId, int photoId);
    Task<ApplicationResponse> GetByIdForApartment(int apartmentId, int id);
    Task<ApplicationResponse> GetAllApartmentPhotos(int apartmentId);
    Task<ApplicationResponse> UpdateApartmentPhoto(int apartmentId, ApartmentPhotoDTO updateApartmentPhotoDTO);
    Task<ApplicationResponse> AddApartmentPhoto(int apartmentId, CreateUpdateApartmentPhotoDTO createApartmentPhotoDTO);
}
