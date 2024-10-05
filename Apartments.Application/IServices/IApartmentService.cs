using Apartments.Application.Common;
using Apartments.Application.Dtos.ApartmentDtos;
using Apartments.Domain.Common;
using Apartments.Domain.QueryFilters;

namespace Apartments.Application.IServices;

public interface IApartmentService
{
    Task<PagedResult<ApartmentDto>> GetAllApartments(ApartmentQueryFilter apartmentQueryFilter);
    Task<ServiceResult<ApartmentDto>> GetApartmentById(int id);
    Task<ServiceResult<ApartmentDto>> CreateApartment(CreateApartmentDto createApartmentDTO);
    Task<ServiceResult<ApartmentDto>> UpdateApartment(int id, UpdateApartmentDto updateApartmentDto);
    Task<ServiceResult<string>> DeleteApartment(int id);
    Task<ServiceResult<string>> RestoreApartment(int id);
    Task<ServiceResult<IEnumerable<ApartmentDto>>> GetOwnedApartments(int? ownerId = null);
}