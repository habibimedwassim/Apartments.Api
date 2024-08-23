using RenTN.Application.DTOs.ApartmentDTOs;
using RenTN.Application.DTOs.ApartmentPhotoDTOs;
using RenTN.Application.Utilities;

namespace RenTN.Application.Services.ApartmentsService;

public interface IApartmentsService
{
    Task<ApplicationResponse> DeleteApartment(int id);
    Task<ApplicationResponse> GetApartmentByID(int id);
    Task<ApplicationResponse> GetApartments();
    Task<ApplicationResponse> GetApartmentPhotos(int id);
    Task<ApplicationResponse> CreateApartment(CreateApartmentDTO createApartmentDTO);
    Task<ApplicationResponse> UpdateApartment(UpdateApartmentDTO updateApartmentDTO);
}