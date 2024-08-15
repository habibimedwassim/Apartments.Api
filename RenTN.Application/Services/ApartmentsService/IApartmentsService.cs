using RenTN.Application.DTOs.ApartmentDTOs;

namespace RenTN.Application.Services.ApartmentsService;

public interface IApartmentsService
{
    Task<IEnumerable<ApartmentDTO>> GetApartments();
    Task<ApartmentDTO?> GetApartmentByID(int id);
    Task<int> CreateApartment(CreateApartmentDTO createApartmentDTO);
}