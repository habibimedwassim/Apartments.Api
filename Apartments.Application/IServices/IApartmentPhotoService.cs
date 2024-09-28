using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentPhotoDtos;

namespace Apartments.Application.IServices;

public interface IApartmentPhotoService
{
    Task<ServiceResult<IEnumerable<ApartmentPhotoDto>>> AddPhotosToApartment(UploadApartmentPhotoDto uploadApartmentPhotoDto);
    Task<ServiceResult<string>> DeletePhotoFromApartment(int photoId, int apartmentId);
    Task<ServiceResult<ApartmentPhotoDto>> GetApartmentPhotoById(int photoId, int apartmentId);
    Task<ServiceResult<IEnumerable<ApartmentPhotoDto>>> GetApartmentPhotos(int apartmentId);
    Task<ServiceResult<string>> RestoreApartmentPhoto(int photoId, int apartmentId);
}