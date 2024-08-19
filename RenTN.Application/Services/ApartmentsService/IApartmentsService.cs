using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Application.DTOs.ApartmentPhotoDTOs;

namespace RenTN.Application.Services.ApartmentsService;

public interface IApartmentsService
{
    Task DeleteApartment(int id);
    Task<ApartmentDTO?> GetApartmentByID(int id);
    Task<IEnumerable<ApartmentDTO>> GetApartments();
    Task<List<ApartmentPhotoDTO>> GetApartmentPhotos(int id);
    Task<CreateApartmentDTO?> CreateApartment(CreateApartmentDTO createApartmentDTO);
    Task<UpdateApartmentDTO?> UpdateApartment(UpdateApartmentDTO updateApartmentDTO);
}